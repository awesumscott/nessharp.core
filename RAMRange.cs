﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace NESSharp.Core;

internal class RAMChunk {
	public Address Start;
	public Address End;
	public RAMChunk(Address start, Address end) {
		Start = start; End = end;
	}
}

public class RAM {
	public RAMRange Zp { get; init; }
	public RAMRange Ram { get; init; }
	public RAM(RAMRange zp, RAMRange ram) {
		Zp = zp;
		Ram = ram;
	}
	public RAM Remainder() => new RAM(Zp.Remainder(), Ram.Remainder());
}
public class RAMRange {
	//TODO: make a way to allocate a chunk
	public string Name = "main";
	private Address _start, _end, _next;
	private List<RAMChunk> Taken = new List<RAMChunk>();
	public RAMRange(Address startOffset, Address endOffset, string name = "") {
		_start = _next = startOffset;
		_end = endOffset;
		Name = name;
	}
	private void ValidateNext(int len = 1) {
		while (true) {
			//if _next is in a taken range,
			var end = (U16)(_next + len - 1);
			var inRange = Taken.Where(x => (_next >= x.Start && _next <= x.End) || (end >= x.Start && end <= x.End)).Take(1);
			if (inRange.Any()) {
				//skip to the end of it
				_next = inRange.First().End.IncrementedValue;
				//then check if its in any other ranges
				continue;
			}
			//and if it's still within the main range
			if (_next > _end)
				throw new Exception("This RAM section is full");
			//then it's fine!
			break;
		}
	}

	public Address[] Dim(int numBytes) {
		ValidateNext(numBytes);
		var addr0 = _next;
		var addrs = new List<Address>(){ addr0 };
		_next = _next.IncrementedValue;
		for (var i = 1; i < numBytes; i++) {
			addrs.Add(_next);
			_next = _next.IncrementedValue;
		}
		return addrs.ToArray();
	}
	public RAMRange Allocate(Address start, Address end, string name) {
		var length = end - start;
		if (Taken.Any(x => (start >= x.Start && start <= x.End) || //start is within an existing range
							(end >= x.Start && end <= x.End) || //end is within an existing range
							(x.Start >= start && x.End <= end))) //existing range is within new range
			throw new Exception("Range already in use");

		Taken.Add(new RAMChunk(start, end));
		return new RAMRange(start, end, name);
		//Taken = Taken.OrderBy(x => x.Start).ToList();
	}

	public RAMRange Remainder(string name = "") => new(_next, _end, name) {
		Taken = new List<RAMChunk>(Taken)
	};

	public int Size => _end - _start;
	//TODO: include Taken sizes
	public int Used => _next - _start;
}
