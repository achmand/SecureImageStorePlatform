namespace Logic.DomainObjects
{
    public enum ProcessResult
    {
        Failure,
        Success,
    }

    // result returned in domain
    public sealed class DomainResult<T>
    {
        public ProcessResult ProcessResult { get; set; }
        public T ObjectResult { get; set; }
        public string MessageResult { get; set; }
    }
}
