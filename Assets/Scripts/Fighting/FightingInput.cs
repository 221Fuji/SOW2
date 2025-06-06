//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.11.2
//     from Assets/PlayerInputActions.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class FightingInput: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public FightingInput()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerInputActions"",
    ""maps"": [
        {
            ""name"": ""Fighting"",
            ""id"": ""1743223c-2511-42a3-b96b-b5d7fc9dff5c"",
            ""actions"": [
                {
                    ""name"": ""FourDirections"",
                    ""type"": ""Value"",
                    ""id"": ""5fc13f59-f0c2-494e-8c95-5b41ea036bd0"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""975803be-6702-4356-934b-3e47a77797e7"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""NomalMove"",
                    ""type"": ""Button"",
                    ""id"": ""a8f9ef1c-9c37-4b5c-864f-ff7df0d10268"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""SpecialMove1"",
                    ""type"": ""Button"",
                    ""id"": ""b961b71c-0689-4b88-820c-dbbbe142042b"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""SpecialMove2"",
                    ""type"": ""Button"",
                    ""id"": ""d6c5efb2-c594-43d4-97ef-6393ea7ec68a"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Guard"",
                    ""type"": ""Value"",
                    ""id"": ""bebe953d-2feb-4451-abb3-08d2e9315315"",
                    ""expectedControlType"": ""Digital"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Ultimate"",
                    ""type"": ""Button"",
                    ""id"": ""fbe694de-eeca-4bd1-92f2-a46f16a27684"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""511d03ee-26f7-441a-87f6-63964202ea16"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""FourDirections"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""8fa46373-2952-4ad1-90ec-73d54007e446"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""FourDirections"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""346e1ab1-c3d0-459e-bb9d-a9cfa92c1d21"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""FourDirections"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""13abc0fa-0902-4777-ad23-bb01e39e205b"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""FourDirections"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""bcd028b8-3898-4b1f-95ae-6b61effc708f"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""FourDirections"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""bf000552-5e6f-4117-9d3e-80f7ec3bb312"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""FourDirections"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""dfc80a54-a119-414b-a8f3-ed6be05e30ef"",
                    ""path"": ""<HID::HORI CO.,LTD. HORIPAD 4 >/hat/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""FourDirections"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""8c8fbb51-59be-40e8-bc9d-b28be3ce139b"",
                    ""path"": ""<HID::HORI CO.,LTD. HORIPAD 4 >/hat/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""FourDirections"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""020ee51c-c041-4604-87e8-d6b195296227"",
                    ""path"": ""<HID::HORI CO.,LTD. HORIPAD 4 >/hat/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""FourDirections"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""3968acc7-e5a9-4f7a-92d0-13465ce73df5"",
                    ""path"": ""<HID::HORI CO.,LTD. HORIPAD 4 >/hat/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""FourDirections"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""c2ae41be-03ed-43eb-a03b-4e360d18f30a"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""FourDirections"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""8cc7e280-f746-4fbf-a8ee-75dec6462596"",
                    ""path"": ""<XInputController>/dpad/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""FourDirections"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""6747ae9e-e7f3-4b0d-bba6-b386820b853e"",
                    ""path"": ""<XInputController>/dpad/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""FourDirections"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""b0b2cd3a-134a-4d5f-9831-e55ae950980c"",
                    ""path"": ""<XInputController>/dpad/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""FourDirections"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""9d791f99-4519-40aa-887b-08211fd40818"",
                    ""path"": ""<XInputController>/dpad/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""FourDirections"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""5174c2d4-0a63-4725-87f3-451ff65e0599"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""93a61797-950e-4716-be91-7901a64d828a"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6ba72158-d196-425b-a478-34f8d7fbcdf3"",
                    ""path"": ""<Gamepad>/dpad/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1f56cec7-e85a-4f8e-82fd-af9043376534"",
                    ""path"": ""<XInputController>/dpad/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f56beb85-4219-4281-9967-a0533237f25c"",
                    ""path"": ""<Keyboard>/j"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""NomalMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""68665d76-c5ee-40a7-b855-1639ea888000"",
                    ""path"": ""<Gamepad>/buttonNorth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""NomalMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8c13e537-3720-4406-a48b-c3b5247c5685"",
                    ""path"": ""<XInputController>/buttonNorth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""NomalMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""036d6fe3-e698-4e6b-bfba-7402436dd3a0"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SpecialMove1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""74d990ce-b33a-4ed6-a099-71a212c5c338"",
                    ""path"": ""<Keyboard>/k"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SpecialMove1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""57ac6ff0-fa27-49e8-8cc8-787bb2fe2eda"",
                    ""path"": ""<XInputController>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SpecialMove1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""65e88e72-6e27-42aa-9a83-0975000235da"",
                    ""path"": ""<Keyboard>/l"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SpecialMove2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""15efeb45-3f10-4fd0-93f7-9aa80ac4429e"",
                    ""path"": ""<XInputController>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SpecialMove2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""704a8452-916c-4e33-96fc-d93ba90b5828"",
                    ""path"": ""<Gamepad>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SpecialMove2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""fe8715a8-c5fc-430c-9f37-2d8a00fa8404"",
                    ""path"": ""<Gamepad>/leftTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Guard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""874e790e-2a8a-4c05-8cd6-caa2cd753bec"",
                    ""path"": ""<XInputController>/leftTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Guard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2ed34ff5-fb39-4dfa-95ec-4814ba70c6eb"",
                    ""path"": ""<Keyboard>/shift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Guard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""98a73a8c-1097-4e57-8d47-7dadbab13bd1"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Ultimate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5fdf8821-f8a3-4440-a76a-cb19a424fd10"",
                    ""path"": ""<XInputController>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Ultimate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e3867e9d-ee07-42ed-a1b1-44caad00de4f"",
                    ""path"": ""<Keyboard>/u"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Ultimate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Fighting
        m_Fighting = asset.FindActionMap("Fighting", throwIfNotFound: true);
        m_Fighting_FourDirections = m_Fighting.FindAction("FourDirections", throwIfNotFound: true);
        m_Fighting_Jump = m_Fighting.FindAction("Jump", throwIfNotFound: true);
        m_Fighting_NomalMove = m_Fighting.FindAction("NomalMove", throwIfNotFound: true);
        m_Fighting_SpecialMove1 = m_Fighting.FindAction("SpecialMove1", throwIfNotFound: true);
        m_Fighting_SpecialMove2 = m_Fighting.FindAction("SpecialMove2", throwIfNotFound: true);
        m_Fighting_Guard = m_Fighting.FindAction("Guard", throwIfNotFound: true);
        m_Fighting_Ultimate = m_Fighting.FindAction("Ultimate", throwIfNotFound: true);
    }

    ~FightingInput()
    {
        UnityEngine.Debug.Assert(!m_Fighting.enabled, "This will cause a leak and performance issues, PlayerInputActions.Fighting.Disable() has not been called.");
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Fighting
    private readonly InputActionMap m_Fighting;
    private List<IFightingActions> m_FightingActionsCallbackInterfaces = new List<IFightingActions>();
    private readonly InputAction m_Fighting_FourDirections;
    private readonly InputAction m_Fighting_Jump;
    private readonly InputAction m_Fighting_NomalMove;
    private readonly InputAction m_Fighting_SpecialMove1;
    private readonly InputAction m_Fighting_SpecialMove2;
    private readonly InputAction m_Fighting_Guard;
    private readonly InputAction m_Fighting_Ultimate;
    public struct FightingActions
    {
        private FightingInput m_Wrapper;
        public FightingActions(FightingInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @FourDirections => m_Wrapper.m_Fighting_FourDirections;
        public InputAction @Jump => m_Wrapper.m_Fighting_Jump;
        public InputAction @NomalMove => m_Wrapper.m_Fighting_NomalMove;
        public InputAction @SpecialMove1 => m_Wrapper.m_Fighting_SpecialMove1;
        public InputAction @SpecialMove2 => m_Wrapper.m_Fighting_SpecialMove2;
        public InputAction @Guard => m_Wrapper.m_Fighting_Guard;
        public InputAction @Ultimate => m_Wrapper.m_Fighting_Ultimate;
        public InputActionMap Get() { return m_Wrapper.m_Fighting; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(FightingActions set) { return set.Get(); }
        public void AddCallbacks(IFightingActions instance)
        {
            if (instance == null || m_Wrapper.m_FightingActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_FightingActionsCallbackInterfaces.Add(instance);
            @FourDirections.started += instance.OnFourDirections;
            @FourDirections.performed += instance.OnFourDirections;
            @FourDirections.canceled += instance.OnFourDirections;
            @Jump.started += instance.OnJump;
            @Jump.performed += instance.OnJump;
            @Jump.canceled += instance.OnJump;
            @NomalMove.started += instance.OnNomalMove;
            @NomalMove.performed += instance.OnNomalMove;
            @NomalMove.canceled += instance.OnNomalMove;
            @SpecialMove1.started += instance.OnSpecialMove1;
            @SpecialMove1.performed += instance.OnSpecialMove1;
            @SpecialMove1.canceled += instance.OnSpecialMove1;
            @SpecialMove2.started += instance.OnSpecialMove2;
            @SpecialMove2.performed += instance.OnSpecialMove2;
            @SpecialMove2.canceled += instance.OnSpecialMove2;
            @Guard.started += instance.OnGuard;
            @Guard.performed += instance.OnGuard;
            @Guard.canceled += instance.OnGuard;
            @Ultimate.started += instance.OnUltimate;
            @Ultimate.performed += instance.OnUltimate;
            @Ultimate.canceled += instance.OnUltimate;
        }

        private void UnregisterCallbacks(IFightingActions instance)
        {
            @FourDirections.started -= instance.OnFourDirections;
            @FourDirections.performed -= instance.OnFourDirections;
            @FourDirections.canceled -= instance.OnFourDirections;
            @Jump.started -= instance.OnJump;
            @Jump.performed -= instance.OnJump;
            @Jump.canceled -= instance.OnJump;
            @NomalMove.started -= instance.OnNomalMove;
            @NomalMove.performed -= instance.OnNomalMove;
            @NomalMove.canceled -= instance.OnNomalMove;
            @SpecialMove1.started -= instance.OnSpecialMove1;
            @SpecialMove1.performed -= instance.OnSpecialMove1;
            @SpecialMove1.canceled -= instance.OnSpecialMove1;
            @SpecialMove2.started -= instance.OnSpecialMove2;
            @SpecialMove2.performed -= instance.OnSpecialMove2;
            @SpecialMove2.canceled -= instance.OnSpecialMove2;
            @Guard.started -= instance.OnGuard;
            @Guard.performed -= instance.OnGuard;
            @Guard.canceled -= instance.OnGuard;
            @Ultimate.started -= instance.OnUltimate;
            @Ultimate.performed -= instance.OnUltimate;
            @Ultimate.canceled -= instance.OnUltimate;
        }

        public void RemoveCallbacks(IFightingActions instance)
        {
            if (m_Wrapper.m_FightingActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IFightingActions instance)
        {
            foreach (var item in m_Wrapper.m_FightingActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_FightingActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public FightingActions @Fighting => new FightingActions(this);
    public interface IFightingActions
    {
        void OnFourDirections(InputAction.CallbackContext context);
        void OnJump(InputAction.CallbackContext context);
        void OnNomalMove(InputAction.CallbackContext context);
        void OnSpecialMove1(InputAction.CallbackContext context);
        void OnSpecialMove2(InputAction.CallbackContext context);
        void OnGuard(InputAction.CallbackContext context);
        void OnUltimate(InputAction.CallbackContext context);
    }
}
