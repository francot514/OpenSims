/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FSO.Common.Rendering.Framework.IO;
using UnityEngine;
using UnityEngine.UIElements;

namespace FSO.Common.Rendering.Framework.Model
{
    /// <summary>
    /// Contains common information used in the update loop
    /// </summary>
    public class UpdateState
    {
        public bool MouseState;
        public Time Time;

       public UIState UIState = new UIState();

        public bool TouchMode;

        public List<char> FrameTextInput;

        /** A Place to keep shared variables, clears every update cycle **/
        public Dictionary<string, object> SharedData = new Dictionary<string, object>();
        public List<MouseDownEvent> MouseEvents = new List<MouseDownEvent>();

        private Dictionary<KeyDownEvent, long> KeyDownTime = new Dictionary<KeyDownEvent, long>();
        private List<KeyCode> KeyInRepeatMode = new List<KeyCode>();

        public List<KeyCode> NewKeys = new List<KeyCode>();
        public int Depth;

        public void Update()
        {
            NewKeys.Clear();
            Depth = 0;

            /**
             * If a key has been held down for X duration, treat it as if it is newly
             * pressed
             */

            var now = Time;
            string[] allKeyNames = Enum.GetNames(typeof(KeyCode));
            KeyCode[] allKeyCodes = (KeyCode[])Enum.GetValues(typeof(KeyCode));

            for(int i = 0; i < allKeyCodes.Length -1; i++)
            {
                var newPress = Input.GetKeyUp(allKeyCodes[i]);
                if (newPress)
                {
                    
                    NewKeys.Add(allKeyCodes[i]);
                }
                else
                {
                    if (KeyInRepeatMode.Contains(allKeyCodes[i]))
                    {

                        /** How long has it been down? **/
                        if (i > 400000)
                        {
                            /** Its been down long enough, consider it a new key **/
                           
                            NewKeys.Add(allKeyCodes[i]);
                        }
                    }
                    else
                    {
                        /** How long has it been down? **/
                        if (i > 9000000)
                        {
                            /** Its been down long enough, consider it in repeat mode **/
                            
                            KeyInRepeatMode.Add(allKeyCodes[i]);
                        }
                    }
                }
            }
        }
    }
}
