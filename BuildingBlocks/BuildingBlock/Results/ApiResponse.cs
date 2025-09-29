using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildingBlock.Results
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; }

        protected ApiResponse(bool success, T data, string message, List<string> errors)
        {
            Success = success;
            Data = data;
            Message = message;
            Errors = errors ?? [];
        }

        public static ApiResponse<T> Ok(T data, string message = null)
            => new(true, data, message, null);

        public static ApiResponse<T> Fail(string message, List<string> errors = null)
            => new(false, default, message, errors);

        public static ApiResponse<T> Fail(List<string> errors)
            => new(false, default, null, errors);
    }

    // Versão sem generic para respostas sem dados
    public class ApiResponse : ApiResponse<object>
    {
        private ApiResponse(bool success, string message, List<string> errors)
            : base(success, null, message, errors) { }

        public static ApiResponse Ok(string message = "Operação realizada com sucesso")
            => new(true, message, null);

        public static new ApiResponse Fail(string message, List<string> errors = null)
            => new(false, message, errors);

        public static new ApiResponse Fail(List<string> errors)
            => new(false, "Ocorreram erros na operação", errors);
    }
}