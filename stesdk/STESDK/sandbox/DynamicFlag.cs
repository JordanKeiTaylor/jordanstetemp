using System;
using System.Collections.Generic;
using System.Linq;
using Improbable.Worker;
using stesdk.sandbox.Log;

namespace stesdk.sandbox
{
    /// <summary>
    /// A dynamic flag that is able to recompute itself as a function of some number of underlying worker flags.
    /// Arbitrary arity functions are supported internally but the API currently only exposes support for 1 and 2-argument functions.
    /// </summary>
    public class DynamicFlag<T> : IConnectionReceiver
    {
        private readonly NamedLogger _logger;
        private readonly SortedSet<string> _names;
        private readonly List<Action<T>> _changeCallbacks = new List<Action<T>>();
        private readonly Delegate _parser;
        private readonly T _defaultValue;
        private Dictionary<string, string> _values;
        private T _value;
        private IConnection _connection;

        public DynamicFlag(IConnection connection, IDispatcher dispatcher, string name, Func<string, T> parser, T defaultValue)
            : this(connection, dispatcher, new string[] { name }, parser, defaultValue)
        {
        }

        public DynamicFlag(IConnection connection, IDispatcher dispatcher, string name1, string name2, Func<string, string, T> parser, T defaultValue)
            : this(connection, dispatcher, new string[] { name1, name2 }, parser, defaultValue)
        {
        }

        public DynamicFlag(IDispatcher dispatcher, string name, Func<string, T> parser, T defaultValue)
            : this(null, dispatcher, new string[] { name }, parser, defaultValue)
        {
        }

        public DynamicFlag(IDispatcher dispatcher, string name1, string name2, Func<string, string, T> parser, T defaultValue)
            : this(null, dispatcher, new string[] { name1, name2 }, parser, defaultValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:stesdk.sandbox.DynamicFlag`1"/> class from a single worker flag.
        /// </summary>
        /// <param name="connection">Connection.</param>
        /// <param name="dispatcher">Dispatcher.</param>
        /// <param name="name">Worker flag name.</param>
        /// <param name="parser">Worker flag parser.</param>
        /// <param name="defaultValue">Default value.</param>
        public DynamicFlag(IConnection connection, Dispatcher dispatcher, string name, Func<string, T> parser, T defaultValue)
            : this(connection, new DispatcherWrapper(dispatcher), new string[] { name }, parser, defaultValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:stesdk.sandbox.DynamicFlag`1"/> class dependent on two worker flags.
        /// </summary>
        /// <param name="connection">Connection.</param>
        /// <param name="dispatcher">Dispatcher.</param>
        /// <param name="name1">Worker flag 1 name.</param>
        /// <param name="name2">Worker flag 2 name.</param>
        /// <param name="parser">Function to parse flag 1 and flag 2.</param>
        /// <param name="defaultValue">Default value.</param>
        public DynamicFlag(IConnection connection, Dispatcher dispatcher, string name1, string name2, Func<string, string, T> parser, T defaultValue)
            : this(connection, new DispatcherWrapper(dispatcher), new string[] { name1, name2 }, parser, defaultValue)
        {
        }

        private DynamicFlag(IDispatcher dispatcher, IEnumerable<string> names, Delegate parser, T defaultValue)
            : this(null, dispatcher, names, parser, defaultValue)
        {
        }

        private DynamicFlag(IConnection connection, IDispatcher dispatcher, IEnumerable<string> names, Delegate parser, T defaultValue)
        {
            this._logger = Logger.DefaultWithName("DynamicFlag " + string.Concat(names));
            this._names = new SortedSet<string>(names);
            this._parser = parser;
            this._defaultValue = defaultValue;
            this._value = defaultValue;
            dispatcher.OnFlagUpdate(OnFlagUpdate);
            if (connection != null)
            {
                AttachConnection(connection);
            }
        }

        /// <summary>
        /// Flag value.
        /// </summary>
        public T Value
        {
            get { return _value; }
        }

        /// <summary>
        /// Flag default value.
        /// </summary>
        /// <value>The default.</value>
        public T Default
        {
            get { return _defaultValue; }
        }

        public void OnChange(Action<T> callback)
        {
            _changeCallbacks.Add(callback);
        }

        public override string ToString()
        {
            return string.Format("[DynamicFlag: Value={0}, Default={1}]", Value, Default);
        }

        public void AttachConnection(IConnection c)
        {
            _connection = c;
            _values = GetValues(_connection, _names);
            SetValue(Recompute());
        }

        public void DetachConnection(IConnection c)
        {
            _connection = null;
        }

        public void SetValue(T newValue)
        {
            if (!_value.Equals(newValue))
            {
                _value = newValue;
                foreach (Action<T> callback in _changeCallbacks)
                {
                    callback(newValue);
                }
            }
        }

        private static Dictionary<string, string> GetValues(IConnection connection, IEnumerable<string> names)
        {
            if (connection != null)
            {
                return names.Select(name => new Tuple<string, Improbable.Collections.Option<string>>(name, connection.GetWorkerFlag(name)))
                            .Where(t => t.Item2.HasValue)
                            .ToDictionary(t => t.Item1, t => t.Item2.Value);
            }
            else
            {
                return new Dictionary<string, string>();
            }
        }

        private T Recompute()
        {
            if (_names.All(name => _values.ContainsKey(name)))
            {
                var valuesArray = _names.Select(name => this._values[name]).ToArray();

                try
                {
                    return (T)_parser.DynamicInvoke(valuesArray);
                }
                catch (Exception e)
                {
                    _logger.Error("Exception while parsing flag", e);
                    return _defaultValue;
                }
            }

            return _defaultValue;
        }

        private void OnFlagUpdate(FlagUpdateOp op)
        {
            if (_names.Contains(op.Name))
            {
                if (op.Value.HasValue)
                {
                    _values[op.Name] = op.Value.Value;
                    _logger.Info($"Flag updated: {op.Name} = {op.Value.Value}");
                }
                else if (_values.ContainsKey(op.Name))
                {
                    _values.Remove(op.Name);
                    _logger.Info($"Flag unset: {op.Name}");
                }

                SetValue(Recompute());
            }
        }
    }
}
