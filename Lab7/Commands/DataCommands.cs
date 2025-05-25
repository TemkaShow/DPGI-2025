using System.Windows.Input;

namespace Lab7.Commands
{
    public static class DataCommands
    {
        public static RoutedCommand Undo { get; set; }
        public static RoutedCommand New { get; set; }
        public static RoutedCommand Edit { get; set; } 
        public static RoutedCommand Save { get; set; }
        public static RoutedCommand Find { get; set; }
        public static RoutedCommand Delete { get; set; }

        static DataCommands()
        {
            // Undo Command
            InputGestureCollection undoInputs = new InputGestureCollection();
            undoInputs.Add(new KeyGesture(Key.Z, ModifierKeys.Control, "Ctrl+Z"));
            Undo = new RoutedCommand("Undo", typeof(DataCommands), undoInputs);

            // New Command
            InputGestureCollection newInputs = new InputGestureCollection();
            newInputs.Add(new KeyGesture(Key.N, ModifierKeys.Control, "Ctrl+N"));
            New = new RoutedCommand("New", typeof(DataCommands), newInputs);

            // Edit Command
            InputGestureCollection editInputs = new InputGestureCollection();
            editInputs.Add(new KeyGesture(Key.E, ModifierKeys.Control, "Ctrl+E"));
            Edit = new RoutedCommand("Edit", typeof(DataCommands), editInputs);

            // Save Command
            InputGestureCollection saveInputs = new InputGestureCollection();
            saveInputs.Add(new KeyGesture(Key.S, ModifierKeys.Control, "Ctrl+S"));
            Save = new RoutedCommand("Save", typeof(DataCommands), saveInputs);

            // Find Command
            InputGestureCollection findInputs = new InputGestureCollection();
            findInputs.Add(new KeyGesture(Key.F, ModifierKeys.Control, "Ctrl+F"));
            Find = new RoutedCommand("Find", typeof(DataCommands), findInputs);

            // Delete Command (as per lab instructions example)
            InputGestureCollection deleteInputs = new InputGestureCollection();
            deleteInputs.Add(new KeyGesture(Key.D, ModifierKeys.Control, "Ctrl+D"));
            deleteInputs.Add(new KeyGesture(Key.Delete, ModifierKeys.None, "Del"));
            Delete = new RoutedCommand("Delete", typeof(DataCommands), deleteInputs);
        }
    }
}