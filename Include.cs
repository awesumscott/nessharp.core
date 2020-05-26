using System;
using System.Linq;
using System.Reflection;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public static class Include {
		public static void Module(Type classType) {
			//TODO: Each of these should change a context, so all classes within a bank can fill up "code sections", "subs" etc,
			//then they can be written all at once.
			var methods = classType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			foreach (var m in methods.WithAttribute<Dependencies>().Select(x => x.method)) {
				Reset();
				m.Invoke(null, null);
			}
			foreach (var m in methods.WithAttribute<CodeSection>().Select(x => x.method)) {
				Reset();
				Use(m.ToLabel());
				m.Invoke(null, null);
			}
			foreach (var m in methods.WithAttribute<Subroutine>().Select(x => x.method)) {
				//TODO: store all of these labels along with their bank ID in a structure to allow for bank-switch calls.
				//Regular calls and bank-switch calls should be stored as object instances, so they can be expanded by the compiler!
				Reset();
				Use(m.ToLabel());
				m.Invoke(null, null);
				Return();
			}
			foreach (var m in methods.WithAttribute<Interrupt>().Select(x => x.method)) {
				Reset();
				Use(m.ToLabel());
				m.Invoke(null, null);
				CPU6502.RTI();
			}
			foreach (var m in methods.WithAttribute<DataSection>().Select(x => x.method)) {
				Reset();
				Use(m.ToLabel());
				m.Invoke(null, null);
			}
		}
		public static T Module<T>(T obj) {
			Module(obj.GetType()); //add all static methods from this type

			//TODO: Each of these should change a context, so all classes within a bank can fill up "code sections", "subs" etc,
			//then they can be written all at once.
			var methods = obj.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (var m in methods.WithAttribute<Dependencies>().Select(x => x.method)) {
				Reset();
				m.Invoke(obj, null);
			}
			foreach (var m in methods.WithAttribute<DataSection>().Select(x => x.method)) {
				Reset();
				Use(m.ToLabel());
				m.Invoke(obj, null);
			}
			foreach (var m in methods.WithAttribute<CodeSection>().Select(x => x.method)) {
				Reset();
				Use(m.ToLabel());
				m.Invoke(obj, null);
			}
			foreach (var m in methods.WithAttribute<Subroutine>().Select(x => x.method)) {
				//TODO: store all of these labels along with their bank ID in a structure to allow for bank-switch calls.
				//Regular calls and bank-switch calls should be stored as object instances, so they can be expanded by the compiler!
				Reset();
				Use(m.ToLabel());
				m.Invoke(obj, null);
				Return();
			}
			foreach (var m in methods.WithAttribute<Interrupt>().Select(x => x.method)) {
				Reset();
				Use(m.ToLabel());
				m.Invoke(obj, null);
				CPU6502.RTI();
			}
			return obj;
		}
		
		public static void Module_old(params Type[] classTypes) {
			foreach (var t in classTypes)
				Module(t);
		}
		public static void File(string fileName) {
			//TODO: figure out how to pass along attribute's bank ID as the ChrBank index
			//CurrentBank.Write(File.ReadAllBytes(fileName));
			Raw(System.IO.File.ReadAllBytes(fileName));
		}
		public static void Asm(string fileName) {
			//TODO: figure out how to pass along attribute's bank ID as the ChrBank index
			//CurrentBank.Write(File.ReadAllBytes(fileName));
			Parsers.Common.Parse(System.IO.File.ReadAllText(fileName));
		}
	}
}
