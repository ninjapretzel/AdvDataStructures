using System;
using System.Text;

/// <summary> Class that holds an implementation of a log(log(n)) Van Emde Boas Tree </summary>
public class VEBTree {
	/// <summary> Helper to generate masks with <paramref name="nbits"/> bits</summary>
	/// <param name="nbits"> bits to have in mask </param>
	/// <returns> mask of <paramref name="nbits"/> length</returns>
	public static long Mask(int nbits) { return (1L << nbits)-1; }
	/// <summary> Creates a <see cref="string"/> with the lowest <paramref name="cap"/> bits in <paramref name="v"/>. </summary>
	/// <param name="v"> Bits to use </param>
	/// <param name="cap"> Number of Least Significant Bits to use </param>
	/// <returns> <see cref="string"/> holding bits </returns>
	public static string BitString(long v, int cap=64) {
		char[] arr = new char[cap];
		for (int i = 0; i < cap; i++) {
			arr[cap-1-i] = (((v >> i) & 1) == 1) ? '1' : '0';
		}
		return new string(arr);
	}
	/// <summary> Helper struct for handling local universe names </summary>
	public struct Name : IComparable<Name> {
		/// <summary> Actual data bits </summary>
		public long bits { get; private set; }
		/// <summary> Number of data bits inside this name </summary>
		public int size { get; private set; }
		/// <summary> Create a <see cref="Name"/> with the given <paramref name="bits"/> in universe of size <paramref name="size"/> </summary>
		/// <param name="bits"> data bits </param>
		/// <param name="size"> number of bits in universe </param>
		public Name(long bits, int size) { 
			if (size <= 0 || size > 64) { throw new Exception($"Name: Required 0 < size <= 64, was : {size}"); }
			this.bits = bits; 
			this.size = size; 
		}
		/// <summary> Combine two <see cref="Name"/>s into one of higher order </summary>
		/// <param name="low"> <see cref="Name"/> containing low bits </param>
		/// <param name="high"> <see cref="Name"/> containing high bits </param>
		public Name(Name low, Name high) {
			if (low.size != high.size) { throw new Exception($"Name: Required low.size == high.size, had: low={low.Str()} high={high.Str()}"); }
			size = low.size + high.size;
			bits = low.bits | (high.bits << low.size);
		}
		/// <summary> Gets a <see cref="Name"/> holding the high half of the bits <see cref="bits"/> and half the <see cref="size"/> </summary>
		public Name high { get { 
				if (size == 1) { throw new Exception($"Name: Cannot get high bits from {Str()}"); }
				return new Name((bits>>(size/2)) & Mask(size/2), size/2); 
		} }
		/// <summary> Gets a <see cref="Name"/> holding the low half of the bits <see cref="bits"/> and half the <see cref="size"/> </summary>
		public Name low { get {
				if (size == 1) { throw new Exception($"Name: Cannot get low bits from {Str()}"); }
				return new Name(bits & Mask(size / 2), size/2); 
		} }

		/// <inheritdoc/>
		public override bool Equals(object obj) { return (obj is Name n) && (n == this); }
		/// <inheritdoc/>
		public override int GetHashCode() { return bits.GetHashCode() ^ size.GetHashCode(); }
		/// <inheritdoc/>
		public override string ToString() { return $"Name {{ size: {size}, bits: \"{BitString(bits,size)}\" }}"; }
		//public override string ToString() { return $"Name {{ size: {size}, bits: {bits} }}"; }
		/// <summary> Generates a short string for this <see cref="Name"/> </summary>
		/// <returns> Short string representation </returns>
		public string Str() { return $"Name {{ size: {size} }}"; }
		public static bool operator ==(Name a, Name b) { return a.bits == b.bits && a.size == b.size; }
		public static bool operator !=(Name a, Name b) { return !(a==b); }
		public static bool operator >(Name a, Name b) { return a.CompareTo(b) > 0; }
		public static bool operator >=(Name a, Name b) { return a.CompareTo(b) >= 0; }
		public static bool operator <(Name a, Name b) { return a.CompareTo(b) < 0; }
		public static bool operator <=(Name a, Name b) { return a.CompareTo(b) <= 0; }
		/// <summary> Implicitly convert a <see cref="Name"/> into an integer. Only ever used with <see cref="high"/>/<see cref="low"/> half-bits </summary>
		public static implicit operator int(Name n) { return (int)n.bits; } // Only ever done with high/low half-bits. 
		/// <summary> Implicitly convert a tuple into a <see cref="Name"/> for convinience </summary>
		public static implicit operator Name((long a, int b) _) { return new Name(_.a, _.b); }
		/// <summary> Implicitly convert a tuple into a <see cref="Name"/> for convinience </summary>
		public static implicit operator Name((int a, int b) _) { return new Name(_.a, _.b); }
		/// <inheritdoc/>
		public int CompareTo(Name other) {
			if (other.size != size) { throw new Exception($"Name: Not same sizes, cannot compare {Str()} to {other.Str()} "); }
			return bits.CompareTo(other.bits);
		}
	}
		
	/// <summary> Base for nodes in a <see cref="VEBTree"/> </summary>
	public class Node {
		/// <summary> Number of bits this node keeps track of </summary>
		public int size { get; internal set; }
		/// <summary> Number of possible values this node keeps track of </summary>
		public long u { get; internal set; }
		/// <summary> Rank of the node </summary>
		public int rank { get; internal set; }
		/// <summary> Currently tracked minimum value </summary>
		internal Name? min;
		/// <summary> Currently tracked maximum value </summary>
		internal Name? max;
		/// <summary> Default constructor </summary>
		public Node() {
			rank = 0;
			size = 1;
			u = 2;
			min = null;
			max = null;
		}
		/// <summary> See if a given <see cref="Name"/> is a member of this <see cref="Node"/> </summary>
		/// <param name="x"> <see cref="Name"/> to check</param>
		/// <returns> <see cref="true"/> if it is a member, <see cref="false"/> otherwise </returns>
		public virtual bool Member(Name x) {
			if (x.size != size) { throw new Exception($"Node of size {size} dealing with incorrect size name {x}"); }
			if (!max.HasValue) { return false; }
			if (x == max.Value || x == min.Value) { return true; }
			return false;
		}
		/// <summary> Insert a local <see cref="Name"/> into this <see cref="Node"/> </summary>
		/// <param name="x"> <see cref="Name"/> to insert </param>
		public virtual void Insert(Name x) {
			if (x.size != size) { throw new Exception($"Node of size {size} dealing with incorrect size name {x}"); }
			if (!max.HasValue) { min = max = x; return; }
			if (x < min.Value) { min = x; }
			if (x > max.Value) { max = x; }
		}

		/// <summary> Check to see if there is a successor to the given <see cref="Name"/> under this <see cref="Node"/> </summary>
		/// <param name="x"> <see cref="Name"/> to check for successor of </param>
		/// <returns> <see cref="Name"/> of successor or <see cref="null"/> if none found </returns>
		public virtual Name? Successor(Name x) {
			if (x.size != size) { throw new Exception($"Node of size {size} dealing with incorrect size name {x}"); }
			if (x.bits == 0 && max.HasValue && max.Value.bits == 1) {
				return max.Value;
			} 
			return null;
		}
		/// <inheritdoc/>
		public override string ToString() {
			return ToString(1);
		}
		/// <summary> Helper for recursively making pretty <see cref="ToString"/>s </summary>
		/// <param name="ident"> Indent level</param>
		/// <param name="identStr"> Indent string </param>
		/// <returns></returns>
		public virtual string ToString(int ident, string identStr = "\t") {
			string mi = min.HasValue ? ""+min.Value.bits : "null";
			string mx = max.HasValue ? ""+max.Value.bits : "null";
			return $"Node u={u}, size={size}, min={mi}, max={mx}";
		}

	}

	/// <summary> Extended <see cref="Node"/> class that contains recursive children </summary>
	public class ExtNode : Node {
		/// <summary> Single <see cref="Node"/> of size sqrt(<see cref="this.u"/>) holding summary information  </summary>
		internal Node summary;
		/// <summary> Cluster of <see cref="Node"/>s of size sqrt(<see cref="this.u"/>) </summary>
		internal Node[] cluster;

		/// <summary> Constructor. </summary>
		/// <param name="rank"> Rank (tier) of this node in its tree. </param>
		public ExtNode(int rank) : base() {
			if (rank <= 0) { throw new Exception("Rank 0 Node should instead be BaseNode"); }
			this.rank = rank;
			size = 1 << rank;
			u = 1L << (2 * rank);
			// Console.WriteLine($"Created new extNode of rank={rank}, size={size}, u={u}");

			summary = (rank == 1) ? new Node() : new ExtNode(rank - 1);
			cluster = new Node[size];
			for (int i = 0; i < size; i++) {
				cluster[i] = (rank == 1) ? new Node() : new ExtNode(rank - 1);
			}
		}

		/// <inheritdoc/>
		public override bool Member(Name x) {
			if (x.size != size) { throw new Exception($"Node of size {size} dealing with incorrect size name {x}"); }
			if (base.Member(x)) { return true; }
			//Console.WriteLine($"Member check @rank={rank} for {x}");
			return cluster[x.high].Member(x.low);
		}

		/// <inheritdoc/>
		public override void Insert(Name x) {
			if (x.size != size) { throw new Exception($"Node of size {size} dealing with incorrect size name {x}"); }
			if (!min.HasValue) { min = max = x; return; }
			if (x < min.Value) { min = x; }
			if (cluster[x.high].min == null) {
				summary.Insert(x.high);
			}
			cluster[x.high].Insert(x.low);
			if (x > max.Value) { max = x; }
		}

		/// <inheritdoc/>
		public override Name? Successor(Name x) {
			if (x.size != size) { throw new Exception($"Node of size {size} dealing with incorrect size name {x}"); }
			if (min.HasValue && x < min) { return min; }
			Name idx = x.high;
			Name? i = x.high;
			Name? m = cluster[idx].max;
			Name? j = null;
			if (m.HasValue && x.low < m) {
				j = cluster[idx].Successor(x.low);
			} else {
				i = summary.Successor(idx);
				if (!i.HasValue) { return null; }
				j = cluster[i.Value].min;
			}
			return new Name(i.Value, j.Value);
		}

		/// <inheritdoc/>
		public override string ToString() {
			return ToString(1);
		}

		/// <inheritdoc/>
		public override string ToString(int ident, string sr = "\t") {
			string indent = "";
			for (int i = 0; i < ident; i++) { indent += sr; }
			StringBuilder s = new StringBuilder($"{base.ToString(ident,sr)}\n{indent}Summary: {summary.ToString(ident+1,sr)}");
			for (int i = 0; i < size; i++) {
				s.Append($"\n{indent}Cluster{i}: {cluster[i].ToString(ident+1,sr)}");
			}
			return s.ToString();
		}

	} 

	/// <summary> Root node of tree </summary>
	internal Node root;

	/// <summary> Create a <see cref="VEBTree"/> of universe size = <paramref name="u"/> </summary>
	/// <param name="u"> Universe size </param>
	public VEBTree(long u) {
		int size = (int) Math.Ceiling(Math.Log2(u));
		int rank = (int) Math.Ceiling(Math.Log2(size));

		Console.WriteLine($"\n\nStarting VEBTree with u={u}");
		root = (rank == 0) ? new Node() : new ExtNode(rank);
	}

	/// <summary> Insert a given element into this <see cref="VEBTree"/>. </summary>
	/// <param name="v"> Element to insert </param>
	public void Insert(long v) { 
		if (v < 0 || v >= root.u) { throw new Exception($"item {v} is outside of universe u={root.u}"); }
		root.Insert((v, root.size)); 
	}
	/// <summary> See if a given element is a member of this <see cref="VEBTree"/>. </summary>
	/// <param name="v"> Element to check </param>
	/// <returns> <see cref="true"/> if <paramref name="v"/> is a member, otherwise <see cref="false"/> </returns>
	public bool Member(long v) { 
		if (v < 0 || v >= root.u) { throw new Exception($"item {v} is outside of universe u={root.u}"); }
		return root.Member((v, root.size)); 
	}
	/// <summary> See if there is a successor of the given element.</summary>
	/// <param name="v"> Element to check for successor of </param>
	/// <returns> ID of successor if there is one, or -1 if there is no successor. </returns>
	public long Successor(long v) {
		if (v < 0 || v >= root.u) { throw new Exception($"item {v} is outside of universe u={root.u}"); }
		Name? succ = root.Successor((v, root.size));
		return !succ.HasValue ? -1 : succ.Value.bits;
	}

	/// <inheritdoc/>
	public override string ToString() { return $"Root: {root.ToString()}"; }
}

class Program {
	static void Main(string[] args) {
		Console.Clear();
		VEBTree.Name n = (12, 4);

		//Console.WriteLine(""+n);
		//Console.WriteLine(""+n.high);
		//Console.WriteLine(""+n.low);

		var t = new VEBTree(16);
		int[] data = new int[] { 2, 3, 4, 5, 7, 14, 15 };
		Console.WriteLine("Initial Tree: ");
		Console.WriteLine(t);
		for (int i = 0; i < data.Length; i++) {
			Console.WriteLine($"Inserting {data[i]}...");
			t.Insert(data[i]);
		}
		Console.WriteLine(t);
		for (int k = 0; k < 16; k++) {
			Console.WriteLine($"is {k} a member? {t.Member(k)}");
		}

		int[,] twoD = new int[2, 5];
		twoD[1, 4] = 3;
		twoD[0, 2] = 5;


	}
}

