using System.Collections.Generic;
using LevelObjects.PhysicalObjects;

namespace LevelObjects.BrushObjects
{
    /// <summary>
    /// Group created when player is placing down objects
    /// Contains <see cref="BrushObjectBase"/> and passes input events to them
    /// </summary>
    public class BrushGroup : GroupBase<BrushObjectBase>
    {

        public void Stamp()
        {
            PhysicalGroup targetGroup;
            List<PhysicalGroup> overlappingGroups = RetrieveOverlappingGroups();
            if (overlappingGroups.Count > 0)
            {
                targetGroup = overlappingGroups[0];
                for (int i = overlappingGroups.Count - 1; i > 0; i--)
                {
                    targetGroup.MergeWithGroup(overlappingGroups[i]);
                    //OverlappingGroups.RemoveAt(i);
                }
            }
            else
            {
                targetGroup = LevelObjectCreator.CreatePhysicalGroup(Position, Rotation);
            }

            foreach (BrushObjectBase brushObject in ContainedObjects)
            {
                brushObject.Stamp(targetGroup);
            }

            targetGroup.Rebuild();
            targetGroup.State = PhysicalGroupState.Frozen;
        }

        public void Cut()
        {
            PhysicalGroup targetGroup;
            List<PhysicalGroup> overlappingGroups = RetrieveOverlappingGroups();
            foreach (BrushObjectBase brushObject in ContainedObjects)
            {
                brushObject.Cut();
            }

            for (int i = overlappingGroups.Count - 1; i > -1; i--)
            {
                overlappingGroups[i].Rebuild();
                overlappingGroups[i].State = PhysicalGroupState.Frozen;

                if (overlappingGroups[i].ContainedObjects.Count == 0)
                {
                    Destroy(overlappingGroups[i].gameObject);
                    overlappingGroups.RemoveAt(i);
                }
            }
        }

        public List<PhysicalGroup> RetrieveOverlappingGroups()
        {
            List<PhysicalGroup> groups = new List<PhysicalGroup>();
            foreach (BrushObjectBase containedObject in ContainedObjects)
            {
                foreach (PhysicalObjectBase overlappingObject in containedObject.TriggerOverlaps)
                {
                    if(containedObject.OverlapsOnLayers(overlappingObject) && !groups.Contains(overlappingObject.Group))
                        groups.Add(overlappingObject.Group);
                }
            }

            return groups;
        }
    }
}