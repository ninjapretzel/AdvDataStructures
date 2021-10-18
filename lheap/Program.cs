using System;

namespace lheap {
	public class LHeap<T> where T: IComparable<T> {
		public class Node {
			internal int npl;
			internal T value;
			internal Node left;
			internal Node right;
			internal Node() { npl = 0; value = default(T); left = right = null; }
			public Node(T t) { npl = 1; value = t; left = right = SENTINEL; }
			public static implicit operator Node(T val) { return new Node(val); }
			public static implicit operator T(Node n) { return n.value; }
		}
		public static void swap(ref Node a, ref Node b) { var c = a; a=b; b=c; }
		public static Node SENTINEL = new Node();
		public static Node Union(Node a, Node b) {
			if (a == SENTINEL) { return b; }
			if (b == SENTINEL) { return a; }
			if (a.value.CompareTo(b.value) > 0) { swap(ref a, ref b); }
			a.right = Union(a.right, b);
			if (a.left.npl < a.right.npl) { swap(ref a.right, ref a.left); }
			a.npl = 1 + a.right.npl;
			return a;
		}
		Node root;
		public LHeap() { root = SENTINEL; }
		public void Insert(T t) { root = Union(root, t); }
		public T ExtractMin() {
			T ret = root;
			// if (root.left.value.CompareTo(root.right.value) > 0) { swap(ref root.left, ref root.right); }
			root = Union(root.left, root.right);
			return ret;
		}
		public T Min { get { return root; } }
		public bool IsEmpty { get { return root == SENTINEL; } }
	}

	class Program {
		static void Main(string[] args) {
			LHeap<int> ints = new LHeap<int>();
			Random r = new Random();
			for (int i = 0; i < 100; i++) {
				ints.Insert(1000 + r.Next(9000));
			}

			while (!ints.IsEmpty) {
				Console.WriteLine(ints.ExtractMin());
			}
		}
	}
}
