using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomToolkit.UI
{
    public class UINavigateResetTrigger : MonoBehaviour
    {
        public void Trigger()
        {
            UINavigation uiNavigation = UINavigation.Instance;

            uiNavigation.Reset();
        }
    }   
}
