using NESSharp.Core;
using System;

namespace NESSharp.Core {
	public interface IScene {
		public void Load();
		public void Step();
	}
}
