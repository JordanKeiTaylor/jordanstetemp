using System;
using System.Linq;
using System.Collections.Generic;
using Improbable.Worker;

namespace Shared
{
    /// <summary>
    /// A dynamic flag that is able to recompute itself as a function of some number of underlying worker flags. 
    /// Arbitrary arity functions are supported internally but the API currently only exposes support for 1 and 2-argument functions.
    /// </summary>
    public class DynamicFlag<T> : IConnectionReceiver
    {
        private readonly Logger.NamedLogger logger;
        private readonly SortedSet<string> names;
        private Dictionary<string, string> values;
        private T value;
        private readonly Delegate parser;
        private readonly T defaultValue;
        private IConnection connection;
        private readonly List<Action<T>> changeCallbacks = new List<Action<T>>();

        private DynamicFlag(IConnection connection, IDispatcher dispatcher, IEnumerable<string> names, Delegate parser, T defaultValue)
        {
            this.logger = Logger.DefaultWithName("DynamicFlag " + string.Concat(names));
            this.names = new SortedSet<string>(names);
            this.parser = parser;
            this.defaultValue = defaultValue;
            this.value = defaultValue;
            dispatcher.OnFlagUpdate(onFlagUpdate);
            if (connection != null)
            {
                AttachConnection(connection);
            }
        }

        private DynamicFlag(IDispatcher dispatcher, IEnumerable<string> names, Delegate parser, T defaultValue) :
            this(null, dispatcher, names, parser, defaultValue)
        {
        }

        public DynamicFlag(IConnection connection, IDispatcher dispatcher, string name, Func<string, T> parser, T defaultValue) :
           this(connection, dispatcher, new string[] { name }, parser, defaultValue)
        {
        }

        public DynamicFlag(IConnection connection, IDispatcher dispatcher, string name1, string name2, Func<string, string, T> parser, T defaultValue) :
            this(connection, dispatcher, new string[] { name1, name2 }, parser, defaultValue)
        {
        }

        public DynamicFlag(IDispatcher dispatcher, string name, Func<string, T> parser, T defaultValue) :
           this(null, dispatcher, new string[] { name }, parser, defaultValue)
        {
        }

        public DynamicFlag(IDispatcher dispatcher, string name1, string name2, Func<string, string, T> parser, T defaultValue) :
            this(null, dispatcher, new string[] { name1, name2 }, parser, defaultValue)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Shared.DynamicFlag`1"/> class from a single worker flag.
        /// </summary>
        /// <param name="connection">Connection.</param>
        /// <param name="dispatcher">Dispatcher.</param>
        /// <param name="name">Worker flag name.</param>
        /// <param name="parser">Worker flag parser.</param>
        /// <param name="defaultValue">Default value.</param>
        public DynamicFlag(IConnection connection, Dispatcher dispatcher, string name, Func<string, T> parser, T defaultValue) :
            this(connection, new DispatcherWrapper(dispatcher), new string[] { name }, parser, defaultValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Shared.DynamicFlag`1"/> class dependent on two worker flags.
        /// </summary>
        /// <param name="connection">Connection.</param>
        /// <param name="dispatcher">Dispatcher.</param>
        /// <param name="name1">Worker flag 1 name.</param>
        /// <param name="name2">Worker flag 2 name.</param>
        /// <param name="parser">Function to parse flag 1 and flag 2.</param>
        /// <param name="defaultValue">Default value.</param>
        public DynamicFlag(IConnection connection, Dispatcher dispatcher, string name1, string name2, Func<string, string, T> parser, T defaultValue) :
            this(connection, new DispatcherWrapper(dispatcher), new string[] { name1, name2 }, parser, defaultValue)
        {
        }

        /// <summary>
        /// Flag value.
        /// </summary>
        public T Value
        {
            get { return value; }
        }

        /// <summary>
        /// Flag default value.    
        /// </summary>
        /// <value>The default.</value>
        public T Default
        {
            get { return defaultValue; }
        }

        public void OnChange(Action<T> callback)
        {
            changeCallbacks.Add(callback);
        }

        public override string ToString()
        {
            return string.Format("[DynamicFlag: Value={0}, Default={1}]", Value, Default);
        }

        private static Dictionary<string, string> getValues(IConnection connection, IEnumerable<string> names)
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

        private T recompute()
        {
            if (names.All(name => values.ContainsKey(name)))
            {
                var valuesArray = names.Select(name => this.values[name]).ToArray();

                try
                {
                    return (T)parser.DynamicInvoke(valuesArray);
                }
                catch (Exception e)
                {
                    logger.Error("Exception while parsing flag", e);
                    return defaultValue;
                }
            }

            return defaultValue;
        }

        private void onFlagUpdate(FlagUpdateOp op)
        {
            if (names.Contains(op.Name))
            {
                if (op.Value.HasValue)
                {
                    values[op.Name] = op.Value.Value;
                    logger.Info($"Flag updated: {op.Name} = {op.Value.Value}");
                }
                else if (values.ContainsKey(op.Name))
                {
                    values.Remove(op.Name);
                    logger.Info($"Flag unset: {op.Name}");
                }

                setValue(recompute());
            }
        }

        public void AttachConnection(IConnection c)
        {
            connection = c;
            values = getValues(connection, names);
            setValue(recompute());
        }

        public void DetachConnection(IConnection c)
        {
            connection = null;
        }

        public void setValue(T newValue)
        {
            if (!value.Equals(newValue))
            {
                value = newValue;
                foreach (Action<T> callback in changeCallbacks)
                {
                    callback(newValue);
                }
            }
        }
    }
}
