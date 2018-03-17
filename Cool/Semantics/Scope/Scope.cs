﻿using Antlr4.Runtime;
using Cool.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cool.Semantics
{
    class Scope : IScope
    {
        /// <summary>
        /// Information relative to variables.
        /// </summary>
        Dictionary<string, TypeInfo> _variables = new Dictionary<string, TypeInfo>();

        /// <summary>
        /// Information relative to variables.
        /// </summary>
        Dictionary<string, (TypeInfo[] Args, TypeInfo ReturnType)> _functions = new Dictionary<string, (TypeInfo[], TypeInfo)>();

        /// <summary>
        /// Information relative to types in the current scope.
        /// </summary>
        public static Dictionary<string, TypeInfo> DeclaredTypes;

        public IScope Parent { get; set; } = NULL;
        public TypeInfo Type { get; set; } = TypeInfo.NULL;

        static Scope()
        {
            DeclaredTypes = new Dictionary<string, TypeInfo>();

            DeclaredTypes.Add("Object", TypeInfo.NULL);
            DeclaredTypes.Add("Bool", new TypeInfo { Text = "Bool", Parent = DeclaredTypes["Object"] });
            DeclaredTypes.Add("Int", new TypeInfo { Text = "Int", Parent = DeclaredTypes["Object"] });
            DeclaredTypes.Add("String", new TypeInfo { Text = "String", Parent = DeclaredTypes["Object"] });
            DeclaredTypes.Add("IO", new TypeInfo { Text = "IO", Parent = DeclaredTypes["Object"] });

            DeclaredTypes["String"].ClassReference = new ClassNode(-1, -1, "String", "Object");
            DeclaredTypes["String"].ClassReference.Scope.Define("length", new TypeInfo[0], DeclaredTypes["Int"]);
            DeclaredTypes["String"].ClassReference.Scope.Define("concat", new TypeInfo[1] { DeclaredTypes["String"] }, DeclaredTypes["String"]);
            DeclaredTypes["String"].ClassReference.Scope.Define("substr", new TypeInfo[2] { DeclaredTypes["Int"], DeclaredTypes["Int"] }, DeclaredTypes["String"]);
            
            DeclaredTypes["Object"].ClassReference = new ClassNode(0, 0, "Object", "NULL");
            DeclaredTypes["Object"].ClassReference.Scope.Define("abort", new TypeInfo[0], DeclaredTypes["Object"]);
            DeclaredTypes["Object"].ClassReference.Scope.Define("type_name", new TypeInfo[0], DeclaredTypes["String"]);
            DeclaredTypes["Object"].ClassReference.Scope.Define("copy", new TypeInfo[0], DeclaredTypes["Object"]);

            DeclaredTypes["IO"].ClassReference = new ClassNode(-1, -1, "IO", "Object");
            DeclaredTypes["IO"].ClassReference.Scope.Define("out_string", new TypeInfo[1] { DeclaredTypes["String"] }, DeclaredTypes["String"]);
            DeclaredTypes["IO"].ClassReference.Scope.Define("out_int", new TypeInfo[1] { DeclaredTypes["Int"] }, DeclaredTypes["Int"]);
            DeclaredTypes["IO"].ClassReference.Scope.Define("in_string", new TypeInfo[0], DeclaredTypes["String"]);
            DeclaredTypes["IO"].ClassReference.Scope.Define("in_int", new TypeInfo[0], DeclaredTypes["Int"]);
        }

        public bool IsDefined(string name, out TypeInfo type)
        {
            return _variables.TryGetValue(name, out type) ||
                    Parent.IsDefined(name, out type);
        }

        public bool IsDefined(string name, TypeInfo[] args, out TypeInfo type)
        {
            type = TypeInfo.ObjectType;
            if(_functions.ContainsKey(name) && _functions[name].Args.Length == args.Length)
            {
                bool ok = true;
                for (int i = 0; i < args.Length; ++i)
                    //the type of parameters must be equal each one.
                    if ((args[i] != _functions[name].Args[i]))
                        ok = false;
                if(ok)
                {
                    type = _functions[name].ReturnType;
                    return true;
                }
            }

            return Parent.IsDefined(name, args, out type) ||
                   Type.Parent.ClassReference.Scope.IsDefined(name, args, out type);
        }

        public bool IsDefinedType(string name, out TypeInfo type)
        {
            return DeclaredTypes.TryGetValue(name, out type);
        }

        public bool Define(string name, TypeInfo type)
        {
            if (_variables.ContainsKey(name))
                return false;
            _variables.Add(name, type);
            return true;
        }

        public bool Define(string name, TypeInfo[] args, TypeInfo type)
        {
            if (_functions.ContainsKey(name))
                return false;
            _functions[name] = (args, type);
            return true;
        }

        public bool Change(string name, TypeInfo type)
        {
            if (!_variables.ContainsKey(name))
                return false;
            _variables[name] = type;
            return true;
        }

        public IScope CreateChild()
        {
            return new Scope()
            {
                Parent = this,
                Type = this.Type
            };
        }

        #region
        private static NullScope nullScope = new NullScope();

        public static NullScope NULL => nullScope;

        public class NullScope : IScope
        {
            public IScope Parent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public TypeInfo Type { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public bool Change(string name, TypeInfo type)
            {
                return false;
            }

            public IScope CreateChild()
            {
                return new Scope()
                {
                    Parent = NULL,
                    Type = TypeInfo.NULL
                };
            }

            public bool Define(string name, TypeInfo type)
            {
                return false;
            }

            public bool Define(string name, TypeInfo[] args, TypeInfo type)
            {
                return false;
            }

            public bool IsDefined(string name, out TypeInfo type)
            {
                type = TypeInfo.ObjectType;
                return false;
            }

            public bool IsDefined(string name, TypeInfo[] args, out TypeInfo type)
            {
                type = TypeInfo.ObjectType;
                return false;
            }

            public bool IsDefinedType(string name, out TypeInfo type)
            {
                type = TypeInfo.ObjectType;
                return false;
            }
        }
        #endregion

    }
}
