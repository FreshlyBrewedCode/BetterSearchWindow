using System;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace BetterSearchWindow
{
    public class BetterAdvancedDropdown<TPayload> : AdvancedDropdown
    {
        private AdvancedDropdownItem<TPayload> root;
        private Action<TPayload> onItemSelectedCallback;
        
        public BetterAdvancedDropdown(AdvancedDropdownState state, AdvancedDropdownItem<TPayload> root, Action<TPayload> onItemSelectedCallback) : base(state)
        {
            this.root = root;
            this.onItemSelectedCallback = onItemSelectedCallback;
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            var payloadItem = item as AdvancedDropdownItem<TPayload>;
            onItemSelectedCallback?.Invoke(payloadItem.payload);
        }

        public static void Show(Rect buttonRect, AdvancedDropdownItem<TPayload> root, Action<TPayload> onItemSelectedCallback)
        {
            new BetterAdvancedDropdown<TPayload>(new AdvancedDropdownState(), root, onItemSelectedCallback)
                .Show(buttonRect);
        }
    }
    
    public class AdvancedDropdownItem<TPayload> : AdvancedDropdownItem
    {
        public TPayload payload;
        public AdvancedDropdownItem(string name) : base(name) { }
    }
}