using DirectShowLib;

namespace Splicer.Utils
{
    /// <summary>
    /// Represents query information for a single ping
    /// </summary>
    public class PinQueryInfo
    {
        private PinDirection _direction;
        private string _name;
        private string _queryId;

        public PinQueryInfo(PinDirection direction, string name, string queryId)
        {
            _direction = direction;
            _name = name;
            _queryId = queryId;
        }

        public PinDirection Direction
        {
            get { return _direction; }
        }

        public string Name
        {
            get { return _name; }
        }

        public string QueryId
        {
            get { return _queryId; }
        }

        public override string ToString()
        {
            return string.Format("Direction: {0}, Name: {1}, QueryId: {2}", Direction, Name, QueryId);
        }
    }
}