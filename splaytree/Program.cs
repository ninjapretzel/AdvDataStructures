using System;
using System.Text;

namespace splaytree {
	class Program {


		static void Main(string[] args) {
			Console.Clear();
			SplayTree<int> t = new SplayTree<int>();
			int max = 10;
			for (int i = 0; i <= max; i++) {
				t.Insert(i);
			}
			Console.WriteLine($"Initialized Tree={t}");
			for (int i = 0; i <= max/2; i++) {
				bool foundi = t.Find(i);
				bool foundmaxi = t.Find(max - i);
				Console.WriteLine($"Found {i}: {foundi}");
				Console.WriteLine($"Found {max - i}: {foundmaxi}");
			}
			Console.WriteLine($"After many finds={t}");

		}

	}

	public class SplayTree<T> where T: IComparable<T> {
		public class Node {
			internal T value;
			internal Node left;
			internal Node right;
			internal Node parent;
			public Node(T val) { value = val; left = right = parent = null; }
			public static implicit operator Node(T val) { return new Node(val); }
			public static implicit operator T(Node n) { return n.value; }
			public static implicit operator bool(Node n) { return n != null; }
			public override string ToString() {
				StringBuilder str = new StringBuilder();
				ToString(0, "\t", str);	
				return str.ToString();
			}
			
			public void ToString(int ident, string identStr = "\t", StringBuilder str = null) {
				if (str == null) { str = new StringBuilder(); }
				string indent = "";
				for (int i = 0; i < ident; i++) { indent += identStr; }

				str.Append(indent);
				str.Append("Value:"); 
				str.Append(value);
				str.Append("\n"); 
				str.Append(indent);
				str.Append("Parent:"); 
				str.Append((parent!=null)?parent.value.ToString():"(null)");

				if (left != null) { 
					str.Append("\n"); 
					str.Append(indent); 
					str.Append("Left:\n"); 
					left.ToString(ident+1, identStr, str); 
					str.Append("\n"); 
				} else { str.Append($"\n{indent}Left: (null)"); }
				if (right != null) { 
					str.Append("\n"); 
					str.Append(indent); 
					str.Append("Right:\n"); 
					right.ToString(ident+1, identStr, str); 
					str.Append("\n");
				} else { str.Append($"\n{indent}Right: (null)"); }


			}
		}

		public Node root;
		public SplayTree() {
			root = null;
		}

		static void swap(ref Node a, ref Node b) { var c = a; a = b; b = c; }

		private Node Union(Node a, Node b) {
			
			return null;
		}
		
		public bool Find(T val) {
			root = Splay(val);
			return (val.CompareTo(root.value) == 0);
		}

		public void Insert(T val) {
			if (root == null) { root = val; }
			else {
				root = Splay(val);
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

		public bool Delete(T val) {
			root = Splay(val);
			if (val.CompareTo(root) == 0) {
				root = Union(root.left, root.right);
				return true;
			}
			return false;
		}

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
		
		public Node Splay(T val, Node root = null) {
			
			if (root == null) { root = this.root; }
			if (!root) { return root; }

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
		public override string ToString() {
			return root.ToString();
		}

	}

}
