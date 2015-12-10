using ReactiveUI;
using System.Collections.Generic;

namespace SimpleNotes.Framework.Reactive
{
    public class NoRangeReactiveList<T> : ReactiveList<T>
    {
        public override void AddRange(IEnumerable<T> collection)
        {
            // ReactiveList's AddRange fires a collection changed event with multiple items, which breaks WPF
            foreach (var item in collection)
            {
                Add(item);
            }
        }
    }
}
