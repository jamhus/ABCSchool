namespace Shared.Wrappers
{
    public class ResponseWrapper : IResponseWrapper
    {
        public List<string> Messages { get; set; } = [];
        public bool IsSuccessful { get; set; }

        public ResponseWrapper() { }

        #region fail

        public static IResponseWrapper Fail()
        {
            return new ResponseWrapper
            {
                IsSuccessful = false
            };
        }

        public static IResponseWrapper Fail(string message)
        {
            return new ResponseWrapper
            {
                IsSuccessful = false,
                Messages = [message]
            };
        }

        public static IResponseWrapper Fail(List<string> messages)
        {
            return new ResponseWrapper
            {
                IsSuccessful = false,
                Messages = messages
            };
        }

        public static Task<IResponseWrapper> FailAsync()
        {
            return Task.FromResult(Fail());
        }

        public static Task<IResponseWrapper> FailAsync(string message)
        {
            return Task.FromResult(Fail(message));
        }

        public static Task<IResponseWrapper> FailAsync(List<string> messages)
        {
            return Task.FromResult(Fail(messages));
        }

        #endregion fail

        #region success
        public static IResponseWrapper Success()
        {
            return new ResponseWrapper
            {
                IsSuccessful = true
            };
        }
        public static IResponseWrapper Success(string message)
        {
            return new ResponseWrapper
            {
                IsSuccessful = true,
                Messages = [message]
            };
        }
        public static IResponseWrapper Success(List<string> messages)
        {
            return new ResponseWrapper
            {
                IsSuccessful = true,
                Messages = messages
            };
        }
        public static Task<IResponseWrapper> SuccessAsync()
        {
            return Task.FromResult(Success());
        }
        public static Task<IResponseWrapper> SuccessAsync(string message)
        {
            return Task.FromResult(Success(message));
        }
        public static Task<IResponseWrapper> SuccessAsync(List<string> messages)
        {
            return Task.FromResult(Success(messages));
        }
        #endregion success
    }

    public class ResponseWrapper<T> : IResponseWrapper<T>
    {
        public T Data { get; set; }
        public List<string> Messages { get; set; } = [];
        public bool IsSuccessful { get; set; }
        public ResponseWrapper() { }

        #region fail
        public static IResponseWrapper<T> Fail()
        {
            return new ResponseWrapper<T>
            {
                IsSuccessful = false
            };
        }
        public static IResponseWrapper<T> Fail(string message)
        {
            return new ResponseWrapper<T>
            {
                IsSuccessful = false,
                Messages = [message]
            };
        }
        public static IResponseWrapper<T> Fail(List<string> messages)
        {
            return new ResponseWrapper<T>
            {
                IsSuccessful = false,
                Messages = messages
            };
        }
        public static Task<IResponseWrapper<T>> FailAsync()
        {
            return Task.FromResult(Fail());
        }
        public static Task<IResponseWrapper<T>> FailAsync(string message)
        {
            return Task.FromResult(Fail(message));
        }
        public static Task<IResponseWrapper<T>> FailAsync(List<string> messages)
        {
            return Task.FromResult(Fail(messages));
        }

        #endregion

        #region success
        public static IResponseWrapper<T> Success(T data)
        {
            return new ResponseWrapper<T>
            {
                IsSuccessful = true,
                Data = data
            };
        }
        public static IResponseWrapper<T> Success(T data, string message)
        {
            return new ResponseWrapper<T>
            {
                IsSuccessful = true,
                Data = data,
                Messages = [message]
            };
        }
        public static IResponseWrapper<T> Success(T data, List<string> messages)
        {
            return new ResponseWrapper<T>
            {
                IsSuccessful = true,
                Data = data,
                Messages = messages
            };
        }
        public static Task<IResponseWrapper<T>> SuccessAsync(T data)
        {
            return Task.FromResult(Success(data));
        }
        public static Task<IResponseWrapper<T>> SuccessAsync(T data, string message)
        {
            return Task.FromResult(Success(data, message));
        }
        public static Task<IResponseWrapper<T>> SuccessAsync(T data, List<string> messages)
        {
            return Task.FromResult(Success(data, messages));
        }
        #endregion success
    }
}