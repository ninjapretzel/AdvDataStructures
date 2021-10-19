using System;
using System.Collections.Generic;


interface IPriorityQueue<T> where T : IComparable<T>, IKeyable {
	void Push(T t);
	T Pop();
	bool Replace(string search, T t);
	bool IsEmpty { get; }
}

class Graph : Dictionary<string, Dictionary<string, int>> { }
class Program {
	static void Main(string[] args) {

		for (int i = 0; i < 100; i++) {
			int size = 10000 * (i+1);
			int edges = size * 5;
			Console.WriteLine($"\n\nPicked problem of size n={size}, m={edges}");
			DateTime beforeMake = DateTime.UtcNow;
			Graph g = MakeGraph(size, edges);
			DateTime afterMake = DateTime.UtcNow;
			Console.WriteLine($"Prepared in {(afterMake-beforeMake).TotalMilliseconds}ms");

			var slow = Dijk(g, "a", false);
			Console.WriteLine($"Solved slowly with result {slow.total}ms, ");
			var fast = Dijk(g, "a", true);
			Console.WriteLine($"Solved fast with result {fast.total}ms");
			
		}
		
	}
	static string Key(int i, int length = 1, string letters = "abcdefghijklmnopqrstuvwxyz") {
		if (length < 1) { length = 1; }
		if (letters == null) { letters = "abcdefghijklmnopqrstuvwxyz"; }
		string key = "";
		while (i >= letters.Length) {
			key = letters[i % letters.Length] + key;
			i = i / letters.Length;
		}
		key = letters[i % letters.Length] + key;
		while (key.Length < length) { key = letters[0] + key; }
		return key;
	}
	static Graph MakeGraph(int n, int m, int min = 1, int max = 5) {
		Graph g = new Graph();
		for (int i = 0; i < n; i++) { g[Key(i)] = new Dictionary<string, int>(); }
		int edges = 0;
		int safety = 0;
		Random r = new Random();
		while (edges < m && safety < 1000) {
			string keyFrom = Key(r.Next(0, n));
			string keyTo = Key(r.Next(0, n));
			if (keyFrom == keyTo) { continue; }
			if (g[keyFrom].ContainsKey(keyTo)) { safety++; continue; }
			g[keyFrom][keyTo] = r.Next(min, max);
			edges++;
			safety = 0;
		}
		if (safety != 0) {
			Console.WriteLine($"Graph creation exited via safetywall after {edges}/{m} requested edges on {n} verts.");
		}
		return g;
	}

	
	class DijkData : IComparable<DijkData>, IKeyable {
		public string key { get; private set; }
		public readonly float cost;
		public int CompareTo(DijkData other) {
			return cost.CompareTo(other.cost);
		}
		public DijkData(string k, float c) {
			key = k;
			cost = c;
		}

	}
	class DijkResult {
		public Dictionary<string, float> distance;
		public Dictionary<string, string> previous;
		public double setup;
		public double time;
		public double total;
	}
	static DijkResult Dijk(Graph graph, string s, bool fast) {
		DateTime start  = DateTime.UtcNow;
		Dictionary<string, float> dist = new Dictionary<string, float>();
		Dictionary<string, string> prev = new Dictionary<string, string>();

		dist[s] = 0;
		List<DijkData> verts = new List<DijkData>();
		foreach (var pair in graph) {
			var vk = pair.Key;
			if (vk!= s) {
				dist[vk] = float.PositiveInfinity;
				prev[vk] = null;
				if (graph[s].ContainsKey(pair.Key)) {
					dist[vk] = graph[s][vk];
				}
			}
			verts.Add(new DijkData(vk, dist[vk]));
		}
		var vs = verts.ToArray();
		IPriorityQueue<DijkData> queue = fast ? new DumbHeap<DijkData>(vs) : new Heap<DijkData>(vs);
		DateTime setup = DateTime.UtcNow;
		while (!queue.IsEmpty) {
			var u = queue.Pop();
			string uk = u.key;
			foreach (var pair in graph[uk]) {
				string vk = pair.Key;
				float alt = dist[uk] + graph[uk][vk];
				if (alt < dist[vk]) {
					dist[vk] = alt;
					prev[vk] = uk;
					queue.Replace(vk, new DijkData(vk, alt));
				}
			}
		}
		DateTime done = DateTime.UtcNow;
		return new DijkResult() {
			distance = dist,
			previous = prev,
			setup = (setup-start).TotalMilliseconds,
			time = (done-setup).TotalMilliseconds,
			total = (done-start).TotalMilliseconds,
		};
	}
	
}

