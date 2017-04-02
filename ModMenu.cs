using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using UnityEngine.EventSystems;
using System.Security.AccessControl;
using System.ComponentModel;

public class ModMenu {

	// A Modular Menu System
	//
	//	a modMenu returns an int to a parent script that tells it which method to use.
	//
	// the modMenu is designed to streamline the creation of game menus using the Unity
	// UI, which, while robust, lacks a generic menu framework familiar to users of 
	// console games and cross-platform games and even smart phone apps.
	//
	// The mod menu is a generic menu template that will fill itself from a directory
	// that you specify via a FILL CODE. Fill codes must be designated by the developer
	// and stored in a database or switchboard class elsewhere in the program.
	// In addition, the methods to be called when an option is chosen from a modMenu
	// live in the classes of the parent screen.

	// There are 4 main types of menus. Each ModMenu must be one of these Types.
	public enum MenuTypes {
		VERTICAL = 0,
		HORIZONTAL = 1,
		SUBMENU = 2,			// a Submenu can appear overlaid on the main Vertical or Horizontal modMenu
		BIMENU = 3				// a BiMenu offers two options -- a positive and a negative
	}

	// Menu Slot height will be set after instantiation
	// from one of these size profiles
	public enum SlotSizeProfiles {
		ECON = 0,
		SMALL = 1,
		LARGE = 2,							// use ConfigSlotSizeProfile() to set the height for each of these sizes
		COMMANDING = 3
	}

	// Fields of each ModMenu instance
	private MenuTypes menuType{ get; set; }				// the type of this specific menu
	private SlotSizeProfiles menuSlotSize{ get; set; }		// the slot profile for this menu
	private float menuSlotMargin{ get; set; }				// leave this amt of space between menu slots
	private int menuSlotLimit{ get; set; }
	private bool useBumpButtons{ get; set; }
	private List<String> menuSlots{ get; set; }			// we hold onto a list of Slots copied from the source List.
														// a menu slot is really just a string that occupies an index in this list.
														// this is the code backing the menu options	
	private Transform slotContainer;	// This is what we parent the slot UI objects to.
	// each menu needs a prefab blueprint.
	// these will be loaded from Resources at runtime.





	// STATIC FIELDS //
	// These belong to the mod menu type so they will be reset to their defaults at application start.

	// Values for each slot size profile can be setup with ConfigSlotSizeProfile() 
	private static float sizeEconHeight = 77f;
	private static float sizeSmallHeight = 99f;
	private static float sizeLargeHeight = 111f;
	private static float sizeCommandingHeight = 133f;

	// Default messages for the buttons on a bimenu
	private static string BI_POSITIVE_TEXT = "YES";
	private static string BI_NEGATIVE_TEXT = "NO";

	// To scroll the list of menu slots, their container is BUMPED up or down on the screen.
	// distance to bump the container each time
	private static float bumpDistance = 22f;

	// Tags
	// Be sure to Utilize these Tags in the Editor.
	private static string SLOT_CONTAINER_TAG = "ModMenuSlotContainer";
	private static string SCROLL_AREA_TAG = "ModMenuScrollArea";



	// The primary Constructor for a ModMenu.
	// You must pass a Builder with Set Parameters into this Constructor.
	private ModMenu(Builder builder) {
		menuType = builder.getMenuType();
		menuSlotSize = builder.getSlotSizeProf ();
		menuSlotMargin = builder.getSlotMargin ();
		menuSlotLimit = builder.getSlotLimit ();
		useBumpButtons = builder.getUseButtons ();
		menuSlots = new List<String> ();
	}

	public static ModMenu.Builder makeBuilder () {
		return new Builder ();
	}

	public class Builder {

		private MenuTypes menuType = MenuTypes.VERTICAL;					// Parameters with default values
		private SlotSizeProfiles slotSize = SlotSizeProfiles.SMALL;			//
		private float slotMargin = 10f;										//
		private int slotLimit = 2;											//
		private bool useBumpButtons = false;								// should we make the bump buttons or no?

		// Empty constructor
		public Builder () {
		}

		// Use these to get, set the parameters.
		public void setMenuType (MenuTypes val) {
			menuType = val;
		}
		public MenuTypes getMenuType() {
			return menuType;
		}
		public void setSlotSizeProf (SlotSizeProfiles val) {
			slotSize = val;
		}
		public SlotSizeProfiles getSlotSizeProf() {
			return slotSize;
		}
		public void setSlotMargin (float val) {
			slotMargin = val;
		}
		public float getSlotMargin() {
			return slotMargin;
		}
		public void setSlotLimit (int val) {
			slotLimit = val;
		}
		public int getSlotLimit() {
			return slotLimit;
		}
		public void setUseButtons (bool val) {
			useBumpButtons = val;
		}
		public bool getUseButtons() {
			return useBumpButtons;
		}

			////
			//// Use this function to build and get the new Mod Menu!
		public ModMenu Build() {
			return new ModMenu (this);
		}
	}

	// After building the Mod Menu call fill and pass in the source list.
	public void fill (List<string> source) {

		slotContainer = GameObject.FindWithTag (SLOT_CONTAINER_TAG).GetComponent<Transform>();
		menuSlots = new List<string> ();
		for (int i = 0; i < source.Count; i++) {
			string slot = source[i];
			menuSlots.Add (slot);
			makeSlotGameObject (i);
		}
		if (useBumpButtons) {
			Debug.Log ("Should use bump buttons");
			makeBumpButtons (this);
		}
	}

	private GameObject GetMenuSlotPrefab () {
		string path = "UI/Menu-Slot";
		GameObject prefab = Resources.Load (path) as GameObject;
		return prefab;
	}



	private void makeSlotGameObject (int makeFromIndex) {

		// Locate the canvas.
		GameObject canvas = GameObject.Find("Canvas");
		// Instantiate the slot and parent it to the Slot Container.
		GameObject slot = (GameObject)GameObject.Instantiate (GetMenuSlotPrefab (), slotContainer);
		// Grab the rectTransform from the slot.
		RectTransform rt = slot.GetComponent<RectTransform> ();
		float yVal = 0f;
		float xVal = 0f;
		// Determine where to place this slot according to the menu type.
		// If we are using the bump buttons, we must also account for the space 
		// taken up by the Backward Button.
		Debug.Log("margin " + menuSlotMargin.ToString());
		if (menuType == MenuTypes.VERTICAL 
			|| menuType == MenuTypes.SUBMENU) {
			yVal = (float)(rt.rect.height + 2*menuSlotMargin) * makeFromIndex;
			if (useBumpButtons) {
				yVal += rt.rect.height + menuSlotMargin;
			}
		} else if (menuType == MenuTypes.HORIZONTAL 
			|| menuType == MenuTypes.BIMENU) {
			xVal = (float)(rt.rect.width + 2*menuSlotMargin) * makeFromIndex;
			if (useBumpButtons) {
				xVal += rt.rect.width + menuSlotMargin;
			}
		}

		Vector2 pos = new Vector2 (0f+xVal, 0f-yVal);
		rt.anchoredPosition = pos;
		rt.localScale = Vector2.one;
		rt.SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, GetSlotHeight ());
		Text slotLabel = slot.transform.GetChild (0).GetComponent<Text> ();
		slotLabel.text = menuSlots[makeFromIndex];
		// the object is named by its index
		slot.name = makeFromIndex.ToString();
	}


	private float GetSlotHeight () {

		switch (menuSlotSize) {

		case (SlotSizeProfiles.ECON):
			return sizeEconHeight;
			break;
		case (SlotSizeProfiles.SMALL):
			return sizeSmallHeight;
			break;
		case (SlotSizeProfiles.LARGE):
			return sizeLargeHeight;
			break;
		case (SlotSizeProfiles.COMMANDING):
			return sizeCommandingHeight;
			break;

		// fall through to default if menuSlotSize isn't set correctly
		default: return sizeSmallHeight;
		}
	}

	public static void ConfigSlotSizeProfile (SlotSizeProfiles prof, float slotHeight){

		switch (prof) {

		case (SlotSizeProfiles.ECON):
			sizeEconHeight = slotHeight;
			break;
		case (SlotSizeProfiles.SMALL):
			sizeSmallHeight = slotHeight;
			break;
		case (SlotSizeProfiles.LARGE):
			sizeLargeHeight = slotHeight;
			break;
		case (SlotSizeProfiles.COMMANDING):
			sizeCommandingHeight = slotHeight;
			break;

			// fall through to default if menuSlotSize isn't set correctly
		//default: 88f;
		}
	}


	private void makeBumpButtons (ModMenu menu) {

		GameObject canvas = GameObject.Find("Canvas");
		// How can we have multiple containers in a multi-menu situation?
		slotContainer = GameObject.FindWithTag (SLOT_CONTAINER_TAG).GetComponent<Transform>();
		RectTransform cntnrRt = slotContainer.GetComponent<RectTransform> ();

		// Make Back button
		GameObject bButton = (GameObject)GameObject.Instantiate (GetMenuSlotPrefab (), slotContainer.transform);
		bButton.name = "BackButton";
		//bButton.transform.GetChild (0).GetComponent<Text> ().text = "back";
		// should cling to the top of the container
		RectTransform rtb = bButton.GetComponent<RectTransform>();
		rtb.localScale = Vector3.one;
		Vector2 bPos = new Vector2 (cntnrRt.anchoredPosition.x, cntnrRt.anchoredPosition.y);
		rtb.anchoredPosition = bPos;
		bButton.transform.SetParent (canvas.transform);
		rtb.SetAsLastSibling ();
		Button bB = bButton.GetComponent<Button> ();
		if (menu.menuType == MenuTypes.VERTICAL) {
			bB.onClick.AddListener (() => {
				BumpContainerVertical (menu, true);
			}); 
		} else if (menu.menuType == MenuTypes.HORIZONTAL) {
			bB.onClick.AddListener (() => {
				BumpContainerHorizontal (menu, false);
			});
		}
		// Make Forward button
		GameObject fButton = (GameObject)GameObject.Instantiate (GetMenuSlotPrefab (), slotContainer.transform);
		fButton.name = "ForwardButton";
		//fButton.transform.GetChild (0).GetComponent<Text> ().text = "forward";
		// should cling to the top of the container
		RectTransform rtf = fButton.GetComponent<RectTransform>();
		rtf.localScale = Vector3.one;
		Vector2 fPos = new Vector2 (cntnrRt.anchoredPosition.x, cntnrRt.anchoredPosition.y-cntnrRt.rect.height);
		rtf.anchoredPosition = fPos;
		fButton.transform.SetParent (canvas.transform);
		rtf.SetAsLastSibling ();
		Button fB = fButton.GetComponent<Button> ();
		if (menu.menuType == MenuTypes.VERTICAL) {
			fB.onClick.AddListener (() => {
				BumpContainerVertical (menu, false);
			});
		} else if (menu.menuType == MenuTypes.HORIZONTAL) {
			fB.onClick.AddListener (() => {
				BumpContainerHorizontal(menu, true);
			});
		}
	}
		

	// Moving the container

	private void BumpContainerVertical (ModMenu menu, bool up) {

		if (up) {
			Vector2 pos = new Vector2 (menu.slotContainer.localPosition.x, (menu.slotContainer.localPosition.y + bumpDistance));
			menu.slotContainer.localPosition = pos;
		} else {
			// (down)
			Vector2 pos = new Vector2 (menu.slotContainer.localPosition.x, (menu.slotContainer.localPosition.y - bumpDistance));
			menu.slotContainer.localPosition = pos;
		}
	}

	private void BumpContainerHorizontal (ModMenu menu, bool right) {
		
		if (right) {
			Vector2 pos = new Vector2 ((menu.slotContainer.localPosition.x + bumpDistance), menu.slotContainer.localPosition.y);
			menu.slotContainer.localPosition = pos;
		} else {
			// (left)
			Vector2 pos = new Vector2 ((menu.slotContainer.localPosition.x - bumpDistance), menu.slotContainer.localPosition.y);
			menu.slotContainer.localPosition = pos;
		}
	}




}
