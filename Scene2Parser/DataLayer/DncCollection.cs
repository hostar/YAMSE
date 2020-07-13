using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MafiaSceneEditor.DataLayer
{
    public class DncCollection
    {
        public ObservableCollection<DncCollection> Items { get; set; } = new ObservableCollection<DncCollection>();
        public NodeTag Tag { get; set; }
        public string Name { get; set; }

        public DncCollection(string name)
        {
            Name = name;
        }
    }
}
