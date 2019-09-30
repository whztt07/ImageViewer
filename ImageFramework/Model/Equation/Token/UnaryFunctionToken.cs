﻿using System;

namespace ImageFramework.Model.Equation.Token
{
    internal class UnaryFunctionToken : ValueToken
    {
        private readonly string funcName;
        private readonly ValueToken value;

        public UnaryFunctionToken(string funcName, ValueToken value)
        {
            this.funcName = funcName.ToLower();
            this.value = value;
        }

        public override string ToHlsl()
        {
            // aqcuire opengl function name
            if (!GetOpenglFunction(funcName, out var start, out var end))
                throw new Exception("invalid string as function name: " + funcName);
            return start + value.ToHlsl() + end;
        }

        private static bool GetOpenglFunction(string name, out string front, out string end)
        {
            switch (name)
            {
                case "alpha":
                    front = "(";
                    end = ").aaaa";
                    break;
                case "tosrgb":
                    front = "toSrgb(";
                    end = ")";
                    break;
                case "fromsrgb":
                    front = "fromSrgb(";
                    end = ")";
                    break;
                case "red":
                    front = "(";
                    end = ").rrrr";
                    break;
                case "green":
                    front = "(";
                    end = ").gggg";
                    break;
                case "blue":
                    front = "(";
                    end = ").bbbb";
                    break;
                // original glsl functions
                case "abs":
                case "sin":
                case "cos":
                case "tan":
                case "asin":
                case "acos":
                case "atan":
                case "exp":
                case "log":
                case "exp2":
                case "log2":
                case "sqrt":
                case "sign":
                case "floor":
                case "ceil":
                case "fract":
                    front = name + "(";
                    end = ")";
                    break;
                case "normalize":
                    front = "float2(" + name + "(float3(";
                    end = "), 1.0).xxxy";
                    break;
                case "length":
                    front = "f4(length(float3(";
                    end = ")))";
                    break;
                default:
                    front = null;
                    end = null;
                    return false;
            }
            return true;
        }
    }
}
