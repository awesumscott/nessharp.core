﻿using System;
using System.Linq;
using System.Reflection;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public class Prototypes {
		class TestVals {
			U8 a = new U8(5);
			U8 b = new U8(10);
			public (U8, U8) g() => (a, b);
		}
		public static void BinTest() {
			Include.Module(typeof(TestNesClass));
		}

		public static void Main(string[] args) {
			//var tv = new TestVals();
			//var (c, b) = tv.g();

			//var a = (ObsoleteAttribute)Attribute.GetCustomAttribute(typeof(ROMManager).GetMethod("WriteFile"), typeof(ObsoleteAttribute));
			//Console.WriteLine(a.ToString());

			//ParseClass(typeof(TestNesClass));
			//Console.WriteLine(string.Join(',',

			ROMManager.SetInfinite();

			ROMManager.CompileBin(BinTest);


			ROMManager.WriteToFile(@"test");
		}
	}

	public class Rom {
		[PrgBankDef(0)]
		public static void ChrBank1() {
			Include.Module(typeof(TestNesClass));
		}
	}
	public static class TestNesClass {
		[Subroutine]
		public static void Init() {
			A.Set(0x69);
			X.Set(5);
			Y.Set(6);

			Comment("a==$10 && (x==0 || x == 1)");
			If(All(Any(() => X.Equals(0), () => X.Equals(1)), () => A.Equals(0x10)), () => {
				A.Set(0x69);
				X.Set(0x69);
			});

			//Comment("(x==0 || (a==$10 && x == 1)");
			//If(Any(() => X.Equals(0), All(() => A.Equals(0x10), () => X.Equals(1))), () => {
			//	A.Set(0x69);
			//	X.Set(0x69);
			//});

			Comment("Combined");
			If(	Option(All(() => A.Equals(0x10), Any(() => X.Equals(0), () => X.Equals(1))), () => {
					A.Set(0x69);
					X.Set(0x69);
				}),
				Option(Any(() => X.Equals(0), All(() => A.Equals(0x10), () => X.Equals(1))), () => {
					A.Set(0x69);
					X.Set(0x69);
				})
			);
			Comment("Combined");
			If(	Option(Any(() => X.Equals(0), All(() => A.Equals(0x10), () => X.Equals(1))), () => {
					A.Set(0x69);
					X.Set(0x69);
				}),
				Option(All(() => A.Equals(0x10), Any(() => X.Equals(0), () => X.Equals(1))), () => {
					A.Set(0x69);
					X.Set(0x69);
				})
			);
			Comment("First option has only Anys");
			If(	Option(Any(() => X.Equals(0), Any(() => A.Equals(0x10), () => X.Equals(1))), () => {
					A.Set(0x69);
					X.Set(0x69);
				}),
				Option(All(() => A.Equals(0x10), Any(() => X.Equals(0), () => X.Equals(1))), () => {
					A.Set(0x69);
					X.Set(0x69);
				})
			);
		}
	}
}
