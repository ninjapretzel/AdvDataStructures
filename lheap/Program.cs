using System;

class Program {
	static void Main(string[] args) {
		LHeap<int> ints = new LHeap<int>();
		int n = 1000000;
		DateTime a, b;
		Console.WriteLine($"Generating some {n} values...");
		Random r = new Random();
		int[] ns = new int[n];
		a = DateTime.UtcNow;
		for (int i = 0; i < n; i++) {
			ns[i] = 10000 + r.Next(90000);
		}
		b = DateTime.UtcNow;
		double ms() { return (b-a).TotalMilliseconds; }

		Console.WriteLine($"...done generating in {ms()}ms...\nInserting them into a heap...");
		a = DateTime.UtcNow;
		for (int i = 0; i < n; i++) {
			ints.Insert(ns[i]);
		}
		b = DateTime.UtcNow;
		
		Console.WriteLine($"...done Inserting in {ms()}ms...\nRemoving values from heap...");
		int last = 0;
		a = DateTime.UtcNow;
		while (!ints.IsEmpty) {
			int val = ints.ExtractMin();
			// Console.Write();
			if (val < last) { Console.WriteLine($"Vals {last} -> {val} are out of order!"); }
			last = val;
		}
		b = DateTime.UtcNow;
		Console.WriteLine($"...done removing in {ms()}ms");
	}
}

/// <summary> Leftist heap: A heap that's fast and mergable, albiet with a heavy bias </summary>
/// <typeparam name="T"> Generic type of contents </typeparam>
public class LHeap<T> where T: IComparable<T> {
	/// <summary> Fairly standard binary tree node class for holding heap contents  </summary>
	internal class Node {
		/// <summary> "Weight" value (null pointer length, distance to null) </summary>
		internal int npl;
		/// <summary> Node contents </summary>
		internal T value;
		/// <summary> Left (heavy) side </summary>
		internal Node left;
		/// <summary> Right (light) side </summary>
		internal Node right;
		/// <summary> Sentinel constructor </summary>
		internal Node() { npl = 0; value = default(T); left = right = null; }
		/// <summary> Data constructor </summary>
		internal Node(T t) { npl = 1; value = t; left = right = SENTINEL; }
		/// <summary> Automatic conversion from <see cref="T"/> to <see cref="LHeap{T}.Node"/> </summary>
		public static implicit operator Node(T val) { return new Node(val); }
		/// <summary> Automatic conversion from <see cref="LHeap{T}.Node"/> to <see cref="T"/> </summary>
		public static implicit operator T(Node n) { return n.value; }
	}
	/// <summary> Swap references (to save a few lines of code) </summary>
	internal static void swap(ref Node a, ref Node b) { var c = a; a=b; b=c; }
	/// <summary> Single object for null-sentinel node </summary>
	internal static readonly Node SENTINEL = new Node();
	/// <summary> Union two trees into a single tree. </summary>
	internal static Node Union(Node a, Node b) {
		if (a == SENTINEL) { return b; }
		if (b == SENTINEL) { return a; }
		if (a.value.CompareTo(b.value) > 0) { swap(ref a, ref b); }
		a.right = Union(a.right, b);
		if (a.left.npl < a.right.npl) { swap(ref a.right, ref a.left); }
		a.npl = 1 + a.right.npl;
		return a;
	}
	/// <summary> Root of the heap </summary>
	internal Node root;
	/// <summary> Construct empty tree </summary>
	public LHeap() { root = SENTINEL; }
	/// <summary> Insert a given value into the tree </summary>
	public void Insert(T t) { root = Union(root, t); }
	/// <summary> Remove the minimum value from the tree </summary>
	public T ExtractMin() {
		T ret = root;
		// if (root.left.value.CompareTo(root.right.value) > 0) { swap(ref root.left, ref root.right); }
		root = Union(root.left, root.right);
		return ret;
	}
	/// <summary> Peek at the minimum node </summary>
	public T Min { get { return root; } }
	/// <summary> Test if tree is empty </summary>
	public bool IsEmpty { get { return root == SENTINEL; } }
}
