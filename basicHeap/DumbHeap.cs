using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using System.Runtime.CompilerServices;

/// <summary> Interface for allowing items contained in the `DumbHeap` to have a way to generate their key.</summary>
public interface IKeyable {
	/// <summary> Return a _unique_ string key. </summary>
	string key { get; }
}

/// <summary> Class that implements a min-heap structure for any type that implements <see cref="IComparable{T}"/></summary>
/// <typeparam name="T"> Generic type contained within </typeparam>
public class DumbHeap<T> : IPriorityQueue<T>, IEnumerable<T> where T : IComparable<T>, IKeyable {
	/// <summary> Default capacity of a new Heap </summary>
	public const int DEFAULT_CAPACITY = 20;
	/// <summary> Default growth factor of a new Heap </summary>
	public const float GROWTH = 1.5f;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int Parent(int i) { return (i - 1) / 2; }
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int Left(int i) { return i * 2 + 1; }
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int Right(int i) { return i * 2 + 2; }

	/// <summary> Indexes of keys of items contained in the heap </summary>
	public Dictionary<string, int> indexes;

	/// <summary> Delegate for overriding comparison functions. </summary>
	/// <param name="a"> First parameter </param>
	/// <param name="b"> Second parameter </param>
	/// <returns> 0 when a == b, negative number when a &lt; b, positive number when a &gt b </returns>
	public delegate int Compare(T a, T b);

	/// <summary> Heapify an array in-place. </summary>
	/// <param name="ts"> Array to heapify </param>
	/// <param name="cnt"> Number of elements to heapify. if not provided, entire array length is heapified </param>
	/// <param name="compare"> Optional override comparison function. If not provided, default `<see cref="IComparable{T}.CompareTo(T?)"/> is used. </param>
	public static void Heapify(T[] ts, Dictionary<string, int> indexes, int? cnt = null, Compare compare = null) {
		int n = cnt.HasValue ? cnt.Value : ts.Length;
		for (int i = ts.Length - 1; i >= 0; i--) {
			SiftDown(ts, i, n, indexes, compare);
		}
	}

	/// <summary> Sift a given index upwards. </summary>
	/// <param name="ts"> Array of values to sift </param>
	/// <param name="index"> Index of item </param>
	/// <param name="cnt"> maximum index to consider. </param>
	/// <param name="compare"> Optional override comparison function. If not provided, default `<see cref="IComparable{T}.CompareTo(T?)"/> is used. </param>
	public static void SiftUp(T[] ts, int index, int cnt, Dictionary<string, int> indexes, Compare compare = null) {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		int cmp(T a, T b) { return (compare == null) ? a.CompareTo(b) : compare(a, b); }
		void swapKeys(int a, int b) { indexes[ts[a].key] = b; indexes[ts[b].key] = a; }

		if (index < 0 || index >= cnt) { return; }
		int i = index;
		int parent = Parent(i);
		T t = ts[i];
		while (cmp(t, (ts[parent])) <= 0) {
			swapKeys(i, parent);
			ts[i] = ts[parent];
			ts[parent] = t;
			i = parent;
			parent = Parent(i);
			if (i == 0) { break; }
		}
	}



	/// <summary> Sift a given index downwards. </summary>
	/// <param name="ts"> Array of values to sift </param>
	/// <param name="index"> Index of item </param>
	/// <param name="cnt"> maximum index to consider. </param>
	/// <param name="compare"> Optional override comparison function. If not provided, default `<see cref="IComparable{T}.CompareTo(T?)"/> is used. </param>
	public static void SiftDown(T[] ts, int index, int cnt, Dictionary<string, int> indexes = null, Compare compare = null) {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		int cmp(T a, T b) { return (compare == null) ? a.CompareTo(b) : compare(a, b); }
		void swapKeys(int a, int b) { if (indexes != null) { indexes[ts[a].key] = b; indexes[ts[b].key] = a; } }

		if (index < 0 || index >= cnt) { return; }
		int i = index;
		T t = ts[i];
		while (true) {
			int left = Left(i);
			int right = Right(i);
			if (left >= cnt) { break; }

			if (right < cnt) {
				T tL = ts[left];
				T tR = ts[right];

				if (cmp(tL, tR) <= 0) {
					if (cmp(t, tL) > 0) {
						swapKeys(i, left);
						ts[i] = ts[left];
						ts[left] = t;
						i = left;
						continue;
					} else { break; }
				} else {
					if (cmp(t, tR) > 0) {
						swapKeys(i, right);
						ts[i] = ts[right];
						ts[right] = t;
						i = right;
						continue;
					} else { break; }
				}
			} else {
				if (cmp(t, ts[left]) > 0) {
					swapKeys(i, left);
					ts[i] = ts[left];
					ts[left] = t;
					i = left;
				} else { break; }
			}
		}

	}

	/// <summary> Current number of items in the heap </summary>
	public int Count { get { return cnt; } }
	/// <summary> Is the heap currently empty? </summary>
	public bool IsEmpty { get { return cnt == 0; } }

	/// <summary> Public access to comparator. Re-heapifies internal array on every write. </summary>
	public Compare Comparator {
		get { return comparator; }
		set {
			comparator = value;
			Heapify(ts, indexes, cnt, comparator);
		}
	}

	/// <summary> Internal comparator. </summary>
	private Compare comparator;

	/// <summary> Current items in heap </summary>
	private T[] ts;
	/// <summary> Current count field </summary>
	private int cnt;

	/// <summary> Empty constructor </summary>
	public DumbHeap() {
		ts = new T[DEFAULT_CAPACITY];
		cnt = 0;
		indexes = new Dictionary<string, int>();
	}

	public DumbHeap(Compare cmp) : this() {
		comparator = cmp;
	}

	/// <summary> Copy constructor </summary>
	/// <param name="ts"> Array of values to copy </param>
	public DumbHeap(T[] ts, Compare cmp = null) {
		this.ts = new T[ts.Length];
		Array.Copy(ts, this.ts, ts.Length);
		cnt = ts.Length;
		comparator = cmp;
		indexes = new Dictionary<string, int>();
		for (int i = 0; i < ts.Length; i++) {
			indexes[ts[i].key] = i;
		}
		Heapify(this.ts, indexes, cnt, comparator);
		CheckKeys();
	}

	private void DoReplace(int index, T replace) {
		int cmp(T a, T b) { return (comparator == null) ? a.CompareTo(b) : comparator(a, b); }
		T old = ts[index];
		ts[index] = replace;
		if (cmp(replace, old) < 0) { SiftUp(index); }
		else if (cmp(replace, old) > 0) { SiftDown(index); }
	}

	public bool Replace(string search, T replace) {
		if (indexes.ContainsKey(search)) {
			var where = indexes[search];
			DoReplace(where, replace);
			return true;
		}
		return false;
	}

	private void SwapKeys(int a, int b) {
		indexes[ts[a].key] = b;
		indexes[ts[b].key] = a;
	}

	private void CheckKeys() {
		for (int i = 0; i < cnt; i++) {
			T thing = ts[i];
			string key = thing.key;
			int index = indexes[key];
			if (index != i) { throw new Exception($"Key {key} thinks it should be at {index}, but it is at {i}!"); }
		}
	}

	/// <summary> Returns the minimal element in the heap </summary>
	/// <returns> Element at position 0 in heap </returns>
	public T Peek() {
		if (cnt == 0) { throw new InvalidOperationException("Heap is empty, cannot Peek."); }
		return ts[0];
	}

	/// <summary> Adds the given element to the heap structure </summary>
	/// <param name="item"> item to add to heap </param>
	public void Push(T item) {
		if (cnt == ts.Length) { Grow(); }
		ts[cnt] = item;
		indexes[item.key] = this.cnt;
		cnt++;
		SiftUp(cnt - 1);
		CheckKeys();
	}

	/// <summary> Removes the minimal element from the heap </summary>
	/// <returns> Element that was previously at position 0 in heap </returns>
	public T Pop() {
		void removeKey(int i) { indexes.Remove(ts[i].key); }
		if (cnt == 0) { throw new InvalidOperationException("Heap is empty, cannot Pop."); }
		
		T t = ts[0];
		if (cnt == 1) {
			removeKey(0);
			ts[0] = default(T);
			cnt = 0;
		} else {
			SwapKeys(0, cnt-1);
			ts[0] = ts[cnt - 1];
			removeKey(cnt-1);
			ts[cnt - 1] = default(T);
			cnt--;
			SiftDown(0);
		}

		return t;
	}

	/// <inheritdoc/>
	public override string ToString() {
		StringBuilder str = new StringBuilder($"Heap<{typeof(T)}> [ ");
		for (int i = 0; i < cnt; i++) {
			str.Append(ts[i]);
			str.Append(", ");
		}
		str.Append("]");
		return str.ToString();
	}

	/// <summary> Internal function to grow <see cref="ts"/> for more space </summary>
	private void Grow() {
		T[] newTs = new T[(int)(ts.Length * GROWTH)];
		Array.Copy(ts, newTs, cnt);
		ts = newTs;
	}

	/// <summary> Internal function to sift upwards </summary>
	/// <param name="index"></param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SiftUp(int index) { SiftUp(ts, index, cnt, indexes, comparator); }

	/// <summary> Internal function to sift downwards </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SiftDown(int index) { SiftDown(ts, index, cnt, indexes, comparator); }

	/// <inheritdoc/>
	public IEnumerator<T> GetEnumerator() { return new Enumerator(this); }
	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() { return new Enumerator(this); }

	public class Enumerator : IEnumerator<T> {
		private int pos;
		private DumbHeap<T> heap;
		public Enumerator(DumbHeap<T> heap) { this.heap = heap; pos = -1; }

		public T Current { get { return heap.ts[pos]; } }
		object IEnumerator.Current { get { return heap.ts[pos]; } }
		public void Dispose() { }
		public bool MoveNext() { return ++pos < heap.Count; }
		public void Reset() { pos = -1; }
	}
}
