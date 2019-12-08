using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Heap<T> where T : IComparable<T>
{
	public int Count { get; private set; }

	private readonly T[] _heap;
	private readonly HashSet<T> _hashSet;

	public Heap(int maxSize = 512)
	{
		_heap = new T[maxSize];
		_hashSet = new HashSet<T>();
	}

	public T RemoveRoot()
	{
		_hashSet.Remove(_heap[0]);
		return _heap[0];
	}

	public void AddChild(T child)
	{
		Count++;
		_hashSet.Add(child);
	}

	public bool Contails(T child)
	{
		return false;
	}

}
