namespace Uchu.Tool.Action
{
    public abstract class BaseAction
    {
        /// <summary>
        /// Directory to download and run Uchu.
        /// </summary>
        public string UchuDirectory { get; }

        /// <summary>
        /// Creates the base action.
        /// </summary>
        /// <param name="uchuDirectory">Directory to download and run Uchu.</param>
        public BaseAction(string uchuDirectory)
        {
            this.UchuDirectory = uchuDirectory;
        }
    }
}