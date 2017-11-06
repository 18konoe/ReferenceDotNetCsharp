using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sample
{
    class Example
    {
        public static void Main()
        {
            Example ex = new Example();
            ex.HookUpDelegate();
        }

        // How to: Hook Up a Delegate Using Reflection
        // https://docs.microsoft.com/en-us/dotnet/framework/reflection-and-codedom/how-to-hook-up-a-delegate-using-reflection
        // https://msdn.microsoft.com/ja-jp/library/ms228976(v=vs.110).aspx
        // 
        private void HookUpDelegate()
        {
            // Load an assembly, for example using the Assembly.Load
            // method. In this case, the executing assembly is loaded, to
            // keep the demonstration simple.
            //
            //Assembly assem = typeof(Example).Assembly;
            Assembly assem = Assembly.LoadFrom("DynamicLibrary.dll");

            // Get the type that is to be loaded, and create an instance 
            // of it. Activator.CreateInstance has other overloads, if
            // the type lacks a default constructor. The new instance
            // is stored as type Object, to maintain the fiction that 
            // nothing is known about the assembly. (Note that you can
            // get the types in an assembly without knowing their names
            // in advance.)
            //
            //Module mod = assem.GetModule("DynamicLibrary.dll");
            //Type tExForm = assem.GetType("DynamicLibrary.ExampleForm");
            Type tExForm = assem.GetType("DynamicLibrary.Library");

            //Object exFormAsObj = Activator.CreateInstance(tExForm);
            dynamic exFormAsObj = Activator.CreateInstance(tExForm);

            // Get an EventInfo representing the Click event, and get the
            // type of delegate that handles the event.
            //
            //EventInfo evClick = tExForm.GetEvent("Click");
            EventInfo evClick = tExForm.GetEvent("Updated");
            Type tDelegate = evClick.EventHandlerType;

            // If you already have a method with the correct signature,
            // you can simply get a MethodInfo for it. 
            //
            MethodInfo miHandler =
                typeof(Example).GetMethod("LuckyHandler",
                    BindingFlags.NonPublic | BindingFlags.Instance);

            // Create an instance of the delegate. Using the overloads
            // of CreateDelegate that take MethodInfo is recommended.
            //
            Delegate d = Delegate.CreateDelegate(tDelegate, this, miHandler);

            // Get the "add" accessor of the event and invoke it late-
            // bound, passing in the delegate instance. This is equivalent
            // to using the += operator in C#, or AddHandler in Visual
            // Basic. The instance on which the "add" accessor is invoked
            // is the form; the arguments must be passed as an array.
            //
            MethodInfo addHandler = evClick.GetAddMethod();
            Object[] addHandlerArgs = { d };
            addHandler.Invoke(exFormAsObj, addHandlerArgs);

            // Event handler methods can also be generated at run time,
            // using lightweight dynamic methods and Reflection.Emit. 
            // To construct an event handler, you need the return type
            // and parameter types of the delegate. These can be obtained
            // by examining the delegate's Invoke method. 
            //
            // It is not necessary to name dynamic methods, so the empty 
            // string can be used. The last argument associates the 
            // dynamic method with the current type, giving the delegate
            // access to all the public and private members of Example,
            // as if it were an instance method.
            //
            Type returnType = GetDelegateReturnType(tDelegate);
            if (returnType != typeof(void))
                throw new ApplicationException("Delegate has a return type.");

            DynamicMethod handler =
                new DynamicMethod("",
                                  null,
                                  GetDelegateParameterTypes(tDelegate),
                                  typeof(Example));

            // Generate a method body. This method loads a string, calls 
            // the Show method overload that takes a string, pops the 
            // return value off the stack (because the handler has no
            // return type), and returns.
            //
            ILGenerator ilgen = handler.GetILGenerator();

            Type[] showParameters = { typeof(String) };
            MethodInfo simpleShow =
                typeof(MessageBox).GetMethod("Show", showParameters);

            ilgen.Emit(OpCodes.Ldstr,
                "This event handler was constructed at run time.");
            ilgen.Emit(OpCodes.Call, simpleShow);
            ilgen.Emit(OpCodes.Pop);
            ilgen.Emit(OpCodes.Ret);

            // Complete the dynamic method by calling its CreateDelegate
            // method. Use the "add" accessor to add the delegate to
            // the invocation list for the event.
            //
            Delegate dEmitted = handler.CreateDelegate(tDelegate);
            addHandler.Invoke(exFormAsObj, new Object[] { dEmitted });

            // Show the form. Clicking on the form causes the two
            // delegates to be invoked.
            //
            //Application.Run((Form)exFormAsObj);

            // Syncronized
            Console.WriteLine("Please input something text:");
            string text = Console.ReadLine();
            exFormAsObj.UpdateAsync(text);

            // Asyncronized
            Console.WriteLine("Please input something text:");
            text = Console.ReadLine();
            Task.Run((() => exFormAsObj.UpdateAsync(text)));

            Console.ReadKey();
        }

        private void LuckyHandler(Object sender, dynamic e)
        {
            MessageBox.Show($"Text: {e.Text}{Environment.NewLine}This event handler just happened to be lying around.");
        }

        private Type[] GetDelegateParameterTypes(Type d)
        {
            if (d.BaseType != typeof(MulticastDelegate))
                throw new ApplicationException("Not a delegate.");

            MethodInfo invoke = d.GetMethod("Invoke");
            if (invoke == null)
                throw new ApplicationException("Not a delegate.");

            ParameterInfo[] parameters = invoke.GetParameters();
            Type[] typeParameters = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                typeParameters[i] = parameters[i].ParameterType;
            }
            return typeParameters;
        }

        private Type GetDelegateReturnType(Type d)
        {
            if (d.BaseType != typeof(MulticastDelegate))
                throw new ApplicationException("Not a delegate.");

            MethodInfo invoke = d.GetMethod("Invoke");
            if (invoke == null)
                throw new ApplicationException("Not a delegate.");

            return invoke.ReturnType;
        }
    }
}
