using System.Collections.Generic;

namespace Game.UnitSelection
{
    internal class Selections
    {
        private readonly List<SelectableComponent> Selected = new List<SelectableComponent>();

        public SelectionUpdatedEvent.SelectAction? UpdateSelections(SelectableComponent selection, bool multi)
        {
            if (IsSelected(selection) && (Selected.Count == 1 || multi))
            {
                return Deselect(selection, multi);
            }
            else
            {
                return Select(selection, multi);
            }
        }

        public SelectionUpdatedEvent.SelectAction? Select(SelectableComponent selection, bool multi)
        {
            if (!multi)
            {
                Clear(Selected.Count == 1 ? selection : null);
            }
            if (selection == null)
            {
                return null;
            }
            if (IsSelected(selection))
            {
                return null;
            }
            Selected.Add(selection);
            selection.Select();
            return SelectionUpdatedEvent.SelectAction.Selected;
        }

        public SelectionUpdatedEvent.SelectAction? Deselect(SelectableComponent selection, bool multi)
        {
            if (!multi)
            {
                Clear();
                return null;
            }
            if (!IsSelected(selection))
            {
                return null;
            }
            Selected.Remove(selection);
            selection.Deselect();
            return SelectionUpdatedEvent.SelectAction.Deselected;
        }

        public SelectionUpdatedEvent.SelectAction? Clear(SelectableComponent ignore = null)
        {
            if (Selected.Count <= 0)
            {
                return null;
            }
            foreach (SelectableComponent selected in Selected)
            {
                if (selected != ignore)
                {
                    selected.Deselect();
                }
            }
            Selected.Clear();
            return SelectionUpdatedEvent.SelectAction.Cleared;
        }

        public bool IsSelected(SelectableComponent selectable)
        {
            return selectable == null ? false : Selected.Contains(selectable);
        }

        public IEnumerable<SelectableComponent> GetSelections()
        {
            return Selected;
        }
    }
}
