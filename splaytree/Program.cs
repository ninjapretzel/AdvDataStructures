using System;
using System.Text;

class Program {

	static void Main(string[] args) {
		Console.Clear();
		SplayTree<int> t = new SplayTree<int>(int.MaxValue);
		int max = 10;
		for (int i = 0; i <= max; i++) {
			t.Insert(i);
		}
		Console.WriteLine($"Initialized Tree=\n{t}");
		for (int i = 0; i <= max/2; i++) {
			bool foundi = t.Find(i);
			bool foundmaxi = t.Find(max - i);
			Console.WriteLine($"Found {i}: {foundi}");
			Console.WriteLine($"Found {max - i}: {foundmaxi}");
		}
		Console.WriteLine($"After many finds=\n{t}");

		Console.WriteLine($"Deleting a 3, {t.Delete(3)}");
		Console.WriteLine($"Tree is now=\n{t}");
		Console.WriteLine($"Deleting a 3, {t.Delete(3)}");
		Console.WriteLine($"Tree is now=\n{t}");

		//Console.WriteLine($"Deleting a 7, {t.Delete(7)}");
		//Console.WriteLine($"Tree is now=\n{t}");
		//Console.WriteLine($"Deleting a 7, {t.Delete(7)}");
		//Console.WriteLine($"Tree is now=\n{t}");

	}

}

/// <summary> Generic splay tree implementation </summary>
/// <typeparam name="T"> Generic type parameter </typeparam>
public class SplayTree<T> where T: IComparable<T> {
	/// <summary> Node class for a splay tree </summary>
	public class Node {
		/// <summary> Stored value </summary>
		internal T value;
		/// <summary> Left child </summary>
		internal Node left;
		/// <summary> Right child </summary>
		internal Node right;
		/// <summary> Parent </summary>
		internal Node parent;
		/// <summary> Constructor. Every node must have a value. </summary>
		/// <param name="val"> Value for node </param>
		public Node(T val) { value = val; left = right = parent = null; }
		/// <summary> Implicitly convert <typeparamref name="T"/> to <see cref="SplayTree{T}.Node"/> </summary>
		/// <param name="val"> Value to convert </param>
		public static implicit operator Node(T val) { return new Node(val); }
		/// <summary> Implicitly convert <see cref="SplayTree{T}.Node"/> to <typeparamref name="T"/> </summary>
		/// <param name="n"> <see cref="Node"/> to convert </param>
		public static implicit operator T(Node n) { return n.value; }
		/// <summary> Implicitly convert <see cref="SplayTree{T}.Node"/> to <see cref="bool"/> </summary>
		/// <param name="n"> <see cref="Node"/> to convert </param>
		public static implicit operator bool(Node n) { return n != null; }
		/// <inheritdoc/>
		public override string ToString() {
			StringBuilder str = new StringBuilder();
			ToString(0, "\t", str);	
			return str.ToString();
		}
			
		/// <summary> Recursive ToString </summary>
		/// <param name="ident"> indent level </param>
		/// <param name="identStr"> indent prefix </param>
		/// <param name="str"> Cached <see cref="StringBuilder"/> for performant string building </param>
		public void ToString(int ident, string identStr = "\t", StringBuilder str = null) {
			if (str == null) { str = new StringBuilder(); }
			string indent = "";
			for (int i = 0; i < ident; i++) { indent += identStr; }

			str.Append($"{indent}Value:{value}\n{indent}Parent:");
			str.Append((parent!=null)?parent.value.ToString():"(null)");

			if (left != null) { 
				str.Append($"\n{indent}Left:\n");
				left.ToString(ident+1, identStr, str); 
				str.Append("\n"); 
			} else { str.Append($"\n{indent}Left: (null)"); }
			if (right != null) { 
				str.Append($"\n{indent}Right:\n"); 
				right.ToString(ident+1, identStr, str); 
				str.Append("\n");
			} else { str.Append($"\n{indent}Right: (null)"); }

		}

	}

	/// <summary> Root of the Tree </summary>
	public Node root;
	/// <summary> Maximum possible value </summary>
	private T max;
	/// <summary> Create a splay tree. 
	/// It cannot infer what the 'max' value is for <typeparamref name="T"/>,
	/// so the maximum must be provided. </summary>
	/// <param name="max"> Maximum or positive infinity. </param>
	public SplayTree(T max) {
		this.max = max;
		root = null;
	}

	/// <summary> Try to find the given value </summary>
	/// <param name="val"> Value to check for </param>
	/// <returns> <see cref="true"/> if present or <see cref="false"/> otherwise </returns>
	public bool Find(T val) {
		Splay(val);
		return (val.CompareTo(root.value) == 0);
	}
	/// <summary> Inserts the value into the tree </summary>
	/// <param name="val"></param>
	public void Insert(T val) {
		if (root == null) { root = val; }
		else {
			Splay(val);
			Node r = val;
			root.parent = r;
			if (val.CompareTo(root) < 0) {
				r.right = root;
			} else {
				r.left = root;
			}
			root = r;
		}
	}

	/// <summary> Deletes the given value from the tree. </summary>
	/// <param name="val"> Value to delete </param>
	/// <returns> <see cref="true"/> if it was present and now deleted or <see cref="false"/> otherwise </returns>
	public bool Delete(T val) {
		Splay(val);
		if (val.CompareTo(root) == 0) {
			root = Union(root.left, root.right);
			return true;
		}
		return false;
	}

	/// <summary> Unions two nodes together. <paramref name="b"/> is anticipated to already be &gt; a </summary>
	/// <param name="a"> First node </param>
	/// <param name="b"> Second node</param>
	/// <returns> Root of new tree formed with M<paramref name="a"/> and <paramref name="b"/> </returns>
	private Node Union(Node a, Node b) {
		if (!a) { return b; }
		if (!b) { return a; }
		Node z = Splay(max, a);
		z.right = b;
		b.parent = z;

		return z;
	}

	/// <summary> "Rotates" <paramref name="x"/> so it becomes the parent of its current parent. </summary>
	/// <param name="x"> Node to rotate </param>
	public static void Rotate(Node x) {
		if (x.parent) {
			var y = x.parent;
			var gp = y.parent;
			if (x == y.left) {
				var b = x.right;
					
				x.right = y;
				y.left = b;
					
				if (gp && y == gp.left) {
					y.parent.left = x;
				} else if (gp && y == gp.right) {
					y.parent.right = x;
				}
				x.parent = y.parent;
				y.parent = x;
				if (b != null) { b.parent = y; }
					
			} else if (x == y.right) { 
				var b = x.left;
				x.left = y;
				y.right = b;

				if (gp && y == gp.left) {
					y.parent.left = x;
				} else if (gp && y == gp.right) {
					y.parent.right = x;
				}
				x.parent = y.parent;
				y.parent = x;
				if (b != null) { b.parent = y; }
					
			} else {
				throw new Exception($"x={x.value} is not a child of y={y.value}!");
			}
		} else {
			throw new Exception($"Error: Tried to rotate parentless node {x.value}");
		}
	}
	
	/// <summary> Splays the root of the entire tree to become <paramref name="val"/> or one of the closest contents to it. </summary>
	/// <param name="val"> Value to splay </param>
	/// <returns> the new Root of the tree</returns>
	private Node Splay(T val) {
		return root = Splay(val, root);
	}

	/// <summary> Splays the root of given subtree <paramref name="val"/> or one of the closest contents to it. </summary>
	/// <param name="val"> Value to splay </param>
	/// <param name="root"> The root of the subtree to consider </param>
	/// <returns> The new root of the subtree </returns>
	public static Node Splay(T val, Node root) {
		Node x = root;
		while (true) {
			int cmp = val.CompareTo(x.value);
			if (cmp == 0) { 
				// Console.WriteLine($"Found node for {val} at node:\n{x}\n"); 
				break; 
			}
				
			Node next = (cmp < 0) ? x.left : x.right;
			if (next == null) { break; }
			x = next;
				
		}

		while (x.parent) {
			// Console.WriteLine($"Rotating from {x}");
			// Zig
			if (!x.parent.parent) { 
				Rotate(x); 
			}
			else {
				if ((x == x.parent.left && x.parent == x.parent.parent.left)
					|| (x == x.parent.right && x.parent == x.parent.parent.right)) {
					// Zig Zigs
					Rotate(x.parent);
					Rotate(x);
				} else {
					// Zig Zags
					Rotate(x);
					Rotate(x);
				}
			}

		}
		return x;
	}
	/// <inheritdoc/>
	public override string ToString() {
		return root.ToString();
	}

}
