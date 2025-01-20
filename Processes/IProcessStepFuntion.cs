namespace PersonalWebApi.Processes
{
    /// <summary>
    /// Use to mark a class as a process step function.
    /// Is used to assigned function to Agent, look Agent Router
    /// </summary>
    public interface IProcessStepFuntion 
    {
        public IEnumerable<string> GetFunctionNames();
    }
}
