using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Glitch.CodeAnalysis.Runtime
{
    public class DynamicCompilationException : Exception
    {
        private IEnumerable<Diagnostic> errors;

        public DynamicCompilationException() : base() { }
        
        public DynamicCompilationException(string message)
            : this(message, null, null) { }
        
        public DynamicCompilationException(string message, Exception innerException) 
            : this(message, null, innerException) { }

        public DynamicCompilationException(IEnumerable<Diagnostic> errors) 
            : this(null, errors) { }

        public DynamicCompilationException(string message, IEnumerable<Diagnostic> errors)
            : base(message) 
        {
            this.errors = errors;
            Message = FormatMessage(message);
        }

        public DynamicCompilationException(string message, IEnumerable<Diagnostic> errors, Exception innerException) 
            : base(message, innerException) 
        {
            this.errors = errors;
            Message = FormatMessage(message);
        }

        protected DynamicCompilationException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public override string Message { get; }

        public IEnumerable<Diagnostic> Errors => errors ?? Enumerable.Empty<Diagnostic>();

        private string FormatMessage(string message)
        {
            if (String.IsNullOrEmpty(message))
            {
                message = "Dynamic compilation failed";
            }

            if (!Errors.Any())
            {
                return message;
            }

            var buffer = new StringBuilder(message); ;
            var indent = new String(' ', 4);

            foreach (var error in Errors)
            {
                buffer.Append(indent)
                        .AppendFormat("{0}: {1}", error.Id, error.GetMessage())
                        .AppendLine();
            }

            return buffer.ToString();
        }
    }
}
