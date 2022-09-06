using System;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro.Nc.UI.EasterEggs
{
    public class SebEasterEgg : EasterEgg
    {
        public string Username;
        private void Start()
        {
            var imageComp = gameObject.GetComponent<Image>();
            var username = Environment.UserName.ToLower();
            Username = username;
            var isNull = imageComp == null;
            if (!isNull && (username.Contains("seb")))
            {
                imageComp.material = Resources.Load<Material>("Mat0");
            }

            var isTrue = username == "jozefina";
            if (!isNull && isTrue)
            {
                imageComp.sprite = Resources.Load<Sprite>("HOT_CAR_AGH");
                imageComp.material = null;
            }

        }

        private void Update()
        {
            // if (Input.GetKeyDown(KeyCode.H))
            // {
            //     ViewSwitcher.ShowAllExceptSelf(this);
            // }
        }
    }
}