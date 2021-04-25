using System;
using ClipperLib;
using LevelObjects;
using LevelObjects.BrushObjects;
using LevelObjects.BrushObjects.MaterialBrushObject;
using LevelObjects.PhysicalObjects;
using UnityEngine;
using UnityEngine.InputSystem;
using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

namespace LevelEditor
{
	public enum LevelEditorMode
	{
		CursorMode = 0,
		HoldingGroupMode,
		BrushMode,
	}

	public class DevLevelEditorManager : SingletonBase
	{
		private LevelEditorMode mode;

		[SerializeField] private Substance[] availableMaterials;
		private Texture[] materialIcons;
		private Paths[] availableBrushShapes;
		[SerializeField] private Texture[] brushShapeIcons;
		private int selectedMaterialIndex;
		private int selectedBrushShapeIndex;

		private BrushGroup brushGroup;
		private MaterialBrushObject brushObject;

		private PhysicalGroup selectedGroup;

		private Camera mainCamera;

		private bool paused;

		protected override void Awake()
		{
			base.Awake();
			mainCamera = Camera.main;

			materialIcons = new Texture[availableMaterials.Length];
			for (int i = 0; i < availableMaterials.Length; i++)
			{
				materialIcons[i] = availableMaterials[i].icon;
			}

			availableBrushShapes = new Paths[4];
			availableBrushShapes[0] = new Paths()
			{
				new Path()
				{
					new IntPoint(-5000, 5000), new IntPoint(5000, 5000), new IntPoint(5000, -5000),
					new IntPoint(-5000, -5000)
				}
			};
			int numPoints = 18;
			availableBrushShapes[1] = new Paths(1);
			availableBrushShapes[1].Add(new Path(36));
			for (int i = 0; i < numPoints; i++)
			{
				float t = 2 * Mathf.PI / numPoints * i;
				availableBrushShapes[1][0].Add(new IntPoint(Mathf.Sin(t) * 5000, Mathf.Cos(t) * 5000));
			}

			availableBrushShapes[2] = new Paths()
			{
				new Path()
				{
					new IntPoint(-5000, -5000), new IntPoint(0, 5000), new IntPoint(5000, -5000)
				}
			};

			availableBrushShapes[3] = new Paths(1);
			availableBrushShapes[3].Add(new Path(10));
			for (int i = 0; i < 10; i++)
			{
				float t = 2 * Mathf.PI / 10 * i;
				int innerOrOuter = 5000;
				if (i % 2 == 1)
				{
					innerOrOuter = 2000;
				}

				availableBrushShapes[3][0].Add(new IntPoint(Mathf.Sin(t) * innerOrOuter, Mathf.Cos(t) * innerOrOuter));
			}
			
#if UNITY_EDITOR
			QualitySettings. vSyncCount = 0; // VSync must be disabled.
			Application.targetFrameRate = Int32.MaxValue;
#endif
		}

		private void EnterCursorMode()
		{
			// Cursor.lockState = CursorLockMode.None;
			mode = LevelEditorMode.CursorMode;
			if (brushGroup != null)
			{
				Destroy(brushGroup.gameObject);
				brushGroup = null;
				brushObject = null;
			}

			if (selectedGroup != null)
			{
				if (paused)
					selectedGroup.State = PhysicalGroupState.Frozen;
				else
					selectedGroup.State = PhysicalGroupState.Physical;
				selectedGroup = null;
			}

			if (!paused)
			{
				PhysicalGroup[] groups = FindObjectsOfType<PhysicalGroup>();
				foreach (PhysicalGroup group in groups)
				{
					group.State = PhysicalGroupState.Physical;
				}
			}
		}

		private void EnterBrushMode()
		{
			if (brushGroup != null)
			{
				Destroy(brushGroup.gameObject);
				brushGroup = null;
				brushObject = null;
			}

			if (mode != LevelEditorMode.BrushMode)
			{
				availableBrushShapes = new Paths[4];
				availableBrushShapes[0] = new Paths()
				{
					new Path()
					{
						new IntPoint(-5000, 5000), new IntPoint(5000, 5000), new IntPoint(5000, -5000),
						new IntPoint(-5000, -5000)
					}
				};
				int numPoints = 18;
				availableBrushShapes[1] = new Paths(1);
				availableBrushShapes[1].Add(new Path(36));
				for (int i = 0; i < numPoints; i++)
				{
					float t = 2 * Mathf.PI / numPoints * i;
					availableBrushShapes[1][0].Add(new IntPoint(Mathf.Sin(t) * 5000, Mathf.Cos(t) * 5000));
				}

				availableBrushShapes[2] = new Paths()
				{
					new Path()
					{
						new IntPoint(-5000, -5000), new IntPoint(0, 5000), new IntPoint(5000, -5000)
					}
				};

				availableBrushShapes[3] = new Paths(1);
				availableBrushShapes[3].Add(new Path(10));
				for (int i = 0; i < 10; i++)
				{
					float t = 2 * Mathf.PI / 10 * i;
					int innerOrOuter = 5000;
					if (i % 2 == 1)
					{
						innerOrOuter = 2000;
					}

					availableBrushShapes[3][0].Add(new IntPoint(Mathf.Sin(t) * innerOrOuter, Mathf.Cos(t) * innerOrOuter));
				}
				
				// Cursor.lockState = CursorLockMode.Locked;
				selectedGroup = null;
				mode = LevelEditorMode.BrushMode;

				var pos = mainCamera.transform.position;
				brushGroup = LevelObjectCreator.CreateBrushGroup(pos, 0);
				brushObject = LevelObjectCreator.CreateMaterialBrushObject(
					availableBrushShapes[selectedBrushShapeIndex], pos, 0, 1, 1,
					availableMaterials[selectedMaterialIndex], brushGroup);
				brushGroup.Rebuild();
			}
		}

		public void PickUpGroup(PhysicalGroup selectedGroup)
		{
			// Cursor.lockState = CursorLockMode.Locked;
			mode = LevelEditorMode.HoldingGroupMode;
			this.selectedGroup = selectedGroup;
			this.selectedGroup.State = PhysicalGroupState.Held;
		}

		private Vector3 mouse, lastMouse, mouseDelta, lastBrushPosition;
		private Path brushPath;
		
		private void Update()
		{
			mouse = Mouse.current.position.ReadValue();
			mouseDelta = Mouse.current.delta.ReadValue();
			mouse.z = -mainCamera.transform.position.z;
			mouseDelta.z = -mainCamera.transform.position.z;
			mouseDelta.x += Screen.width / 2f;
			mouseDelta.y += Screen.height / 2f;
			mouse = mainCamera.ScreenToWorldPoint(mouse);
			mouseDelta = mainCamera.ScreenToWorldPoint(mouseDelta) - mainCamera.transform.position;
			mouse.z = 0;
			mouseDelta.z = 0;

			if (Mouse.current.middleButton.isPressed || Keyboard.current.xKey.isPressed)
			{
				mainCamera.transform.position -= mouseDelta;
			}
			else
			{
				switch (mode)
				{
					case LevelEditorMode.BrushMode:
					{
						if (Keyboard.current.leftCtrlKey.isPressed)
						{
							brushGroup.Scale(mouseDelta.y + 1);
							brushGroup.Rebuild();
						}
						else if (Keyboard.current.leftShiftKey.isPressed)
						{
							brushGroup.transform.eulerAngles += new Vector3(0, 0, -mouseDelta.x * 10);
							//
							// if (Mathf.Abs(mouseDelta.x) > Mathf.Abs(mouseDelta.y))
							// {
							// 	brushGroup.Position =
							// 		new Vector3(mouse.x, brushGroup.Position.y, brushGroup.transform.position.z);
							// }
							// else
							// {
							// 	brushGroup.Position =
							// 		new Vector3(brushGroup.Position.x, mouse.y, brushGroup.transform.position.z);
							// }
						}
						else
						{
							brushGroup.Position =
								new Vector3(mouse.x, mouse.y, brushGroup.transform.position.z);
						}

						brushPath = new Path(3);

						brushPath.Add((Vector2) brushObject.transform.InverseTransformPoint(brushGroup.Position));
						brushPath.Add((Vector2) brushObject.transform.InverseTransformPoint(lastBrushPosition));
						//brushPath.Add((Vector2)_brushObject.transform.InverseTransformPoint(lastLastMouse));

						brushObject.SmearShape(brushPath);

						if (Mouse.current.leftButton.isPressed || Keyboard.current.spaceKey.isPressed)
						{
							brushGroup.Stamp();
						}

						if (Mouse.current.rightButton.isPressed || Keyboard.current.leftAltKey.isPressed)
						{
							brushGroup.Cut();
						}

						lastBrushPosition = brushGroup.Position;
						
						break;
					}
					case LevelEditorMode.HoldingGroupMode:
					{
						if (Keyboard.current.leftShiftKey.isPressed)
						{
							selectedGroup.RotateBy(-mouseDelta.x * 10);
						}
						else
						{
							selectedGroup.MoveBy(mouseDelta);
						}

						break;
					}
				}
			}
			
			lastMouse = mouse;
		}

		public void HandleConfirm(InputAction.CallbackContext context)
		{
			if (!context.performed)
				return;

			switch (mode)
			{
				case LevelEditorMode.CursorMode:

					RaycastHit hit;
					Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadUnprocessedValue());

					if (Physics.Raycast(ray, out hit, 1000))
					{
						PhysicalObjectBase objectHit = hit.transform.GetComponentInParent<PhysicalObjectBase>();
						if (objectHit != null) PickUpGroup(objectHit.Group);
					}

					break;
				case LevelEditorMode.HoldingGroupMode:
					selectedGroup.Glue();
					break;
			}
		}

		public void HandleEscape(InputAction.CallbackContext context)
		{
			if (!context.performed)
				return;
			EnterCursorMode();
		}

		public void HandleTweak(InputAction.CallbackContext context)
		{
			if (!context.performed)
				return;
			EnterBrushMode();
		}

		public void HandleDelete(InputAction.CallbackContext context)
		{
			if (!context.performed)
				return;

			switch (mode)
			{
				case LevelEditorMode.HoldingGroupMode:
					Destroy(selectedGroup.gameObject);
					EnterCursorMode();
					break;
			}
		}

		public void HandleLayerChange(InputAction.CallbackContext context)
		{
			if (!context.performed)
				return;

			int direction = (int) Mathf.Sign(context.ReadValue<float>());

			switch (mode)
			{
				case LevelEditorMode.BrushMode:
					brushGroup.LayerShift(direction);
					brushObject.ApplyShapeModifications();
					break;
				case LevelEditorMode.HoldingGroupMode:
					selectedGroup.LayerShift(direction);
					selectedGroup.Rebuild();
					break;
			}
		}

		public void HandleThicknessChange(InputAction.CallbackContext context)
		{
			if (!context.performed)
				return;

			int direction = (int) Mathf.Sign(context.ReadValue<float>());
			if (mode == LevelEditorMode.BrushMode)
			{
				brushObject.Thickness += direction;
				brushGroup.Rebuild();
			}
		}

		public void HandleSelectMaterial(InputAction.CallbackContext context)
		{
			if (!context.performed)
				return;
			
			if (Keyboard.current.leftCtrlKey.isPressed)
			{
				mainCamera.transform.position += Vector3.forward * (int) Mathf.Sign(context.ReadValue<float>());
			}
			else if (mode == LevelEditorMode.BrushMode)
			{
				if (Keyboard.current.leftShiftKey.isPressed)
				{
					selectedBrushShapeIndex -= (int) Mathf.Sign(context.ReadValue<float>());
					selectedBrushShapeIndex =
						Mathf.Clamp(selectedBrushShapeIndex, 0, availableBrushShapes.Length - 1);
					brushObject.Shape = availableBrushShapes[selectedBrushShapeIndex];
					brushObject.ApplyShapeModifications();
				}
				else
				{
					selectedMaterialIndex -= (int) Mathf.Sign(context.ReadValue<float>());
					selectedMaterialIndex = Mathf.Clamp(selectedMaterialIndex, 0, availableMaterials.Length - 1);
					brushObject.Material = availableMaterials[selectedMaterialIndex];
				}
			}
		}

		public void HandlePause(InputAction.CallbackContext context)
		{
			if (!context.performed)
				return;

			paused = !paused;

			PhysicalGroup[] groups = FindObjectsOfType<PhysicalGroup>();

			foreach (PhysicalGroup group in groups)
			{
				if (selectedGroup == null || group != selectedGroup)
				{
					if (paused)
					{
						group.State = PhysicalGroupState.Frozen;
					}
					else
					{
						group.State = PhysicalGroupState.Physical;
					}
				}
			}
		}

		private int _globalControlHeight = 180;

		private void OnGUI()
		{
			switch (mode)
			{
				case LevelEditorMode.CursorMode:
					GUI.Box(new Rect(10, _globalControlHeight + 20, 220, 70), "Controls: Cursor Mode");
					GUI.Label(new Rect(20, _globalControlHeight + 40, 200, 50),
						"-Click on am object to pick it up. (BUGGY)");
					break;
				case LevelEditorMode.BrushMode:
					GUI.Box(new Rect(10, _globalControlHeight + 20, 220, 220), "Controls: Brush Mode");
					GUI.Label(new Rect(20, _globalControlHeight + 40, 200, 200),
						"-L click/space to stamp\n-R click/L alt to cut\n-Scroll/Q & A to select a material\n-Shift + scroll to select a shape\n-Shift + mouse to rotate brush\n-L ctrl + mouse to scale brush\n-W & S to shift brush between layers\n-E & D to change brush's thickness");

					GUI.SelectionGrid(new Rect(Screen.width - 120, 10, 40, Screen.height - 20),
						selectedBrushShapeIndex,
						brushShapeIcons, 1);
					GUI.SelectionGrid(new Rect(Screen.width - 70, 10, 60, Screen.height - 20), selectedMaterialIndex,
						materialIcons, 1);

					break;
				case LevelEditorMode.HoldingGroupMode:
					GUI.Box(new Rect(10, _globalControlHeight + 20, 220, 100), "Controls: Holding Mode");
					GUI.Label(new Rect(20, _globalControlHeight + 40, 200, 80),
						"-Right click to delete the selected object\n-W & S to shift object between layers");
					break;
			}

			GUI.Box(new Rect(10, 10, 220, _globalControlHeight), "Global controls");
			GUI.Label(new Rect(20, 30, 200, _globalControlHeight - 20),
				"-Drag with M click / drag with X to pan the camera\n-Ctrl & scroll / Ctrl + Q & A to zoom in/out\n-Esc to enter cursor mode\n-Tab to enter brush mode\n-P to toggle pause\nPaused: " +
				paused.ToString() + "\nRough FPS:" + 1f/Time.deltaTime);
		}
	}
}