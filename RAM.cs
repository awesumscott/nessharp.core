using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	internal class RAMChunk {
		public Address Start;
		public Address End;
	}
	public class RAM {
		//TODO: make a way to allocate a chunk
		private Address _start, _end, _next;
		private List<RAMChunk> Taken = new List<RAMChunk>();
		public RAM(Address startOffset, Address endOffset) {
			_start = _next = startOffset;
			_end = endOffset;
		}
		private void ValidateNext() {
			while (true) {
				//if _next is in a taken range,
				var inRange = Taken.Where(x => _next >= x.Start && _next <= x.End).Take(1);
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
		
		public Address Dim() {
			ValidateNext();
			var addr = _next;
			_next = _next.IncrementedValue;
			return addr;
		}
		public Address[] Dim(int numBytes) {
			ValidateNext();
			var addr0 = _next;
			var addrs = new List<Address>(){ addr0 };
			_next = _next.IncrementedValue;
			for (var i = 0; i < numBytes; i++) {
				if (i == 0)
					continue;
				addrs.Add(_next);
				_next = _next.IncrementedValue;
			}
			return addrs.ToArray();
		}
		public RAM Allocate(Address start, Address end) {
			var length = end - start;
			if (Taken.Where(x => (start >= x.Start && start <= x.End) || //start is within an existing range
								(end >= x.Start && end <= x.End) || //end is within an existing range
								(x.Start >= start && x.End <= end)).Any()) //existing range is within new range
				throw new Exception("Range already in use");

			Taken.Add(new RAMChunk(){Start = start, End = end});
			return new RAM(start, end);
			//Taken = Taken.OrderBy(x => x.Start).ToList();
		}

		public int Size => _end - _start;
		public int Used => _next - _start;
	}
}
