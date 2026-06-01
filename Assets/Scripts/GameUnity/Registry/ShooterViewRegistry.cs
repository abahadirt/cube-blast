using System.Collections.Generic;
using Blast.GameUnity.View;


namespace Blast.GameUnity.Registry
{

    // Maps shooterId to ShooterView.
    public class ShooterViewRegistry
    {
        private Dictionary<int, ShooterView> _viewsById = new Dictionary<int, ShooterView>();

        public void Register(int id, ShooterView view)
        {
            _viewsById[id] = view;
        }

        public bool TryGet(int id, out ShooterView view)
        {
            return _viewsById.TryGetValue(id, out view);
        }

        public bool Unregister(int id)
        {
            return _viewsById.Remove(id);
        }
    }


}

