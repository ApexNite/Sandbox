using Sandbox.Toolkit.Graphics;
using Sandbox.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Sandbox.Features {
    internal class InventoryEdit {
        public static void Init() {
            ResourceEditor.CreateWindow("resources_edit", "resources_edit");
            FoodEditor.CreateWindow("food_edit", "food_edit");

            new ButtonBuilder("resources_edit", ButtonStyle.SpecialRed).SetIcon("ui/icons/green_plus")
                .SetWindowId("resources_edit")
                .Build(out PowerButton resourcesEditButton)
                .Next("food_edit", ButtonStyle.SpecialRed)
                .SetIcon("ui/icons/green_plus")
                .SetWindowId("food_edit")
                .Build(out PowerButton foodEditButton);

            GameObject resourcesObject = resourcesEditButton.gameObject;
            GameObject foodObject = foodEditButton.gameObject;
            GameObject spacerPrefab = new GameObject("Spacer");
            GameObject resourcesContentContainer = new GameObject("content");
            GameObject foodContentContainer = new GameObject("content");
            ScrollWindow cityWindow = WindowPreloader.getWindowPrefab("city");
            Transform contentResourcesContainer = cityWindow.transform.FindRecursive("content_resources");
            Transform contentFoodContainer = cityWindow.transform.FindRecursive("content_food");
            Transform resourcesGrid = contentResourcesContainer.Find("content_grid");
            Transform foodGrid = contentFoodContainer.Find("content_grid");
            HorizontalLayoutGroup resourcesLayout = resourcesContentContainer.AddComponent<HorizontalLayoutGroup>();
            HorizontalLayoutGroup foodLayout = foodContentContainer.AddComponent<HorizontalLayoutGroup>();
            LayoutElement resourcesLayoutElement = resourcesObject.AddComponent<LayoutElement>();
            LayoutElement foodLayoutElement = foodObject.AddComponent<LayoutElement>();
            LayoutElement resourcesGridLayoutElement = resourcesGrid.AddComponent<LayoutElement>();
            LayoutElement foodGridLayoutElement = foodGrid.AddComponent<LayoutElement>();
            LayoutElement spacerLayoutElement = spacerPrefab.AddComponent<LayoutElement>();

            resourcesContentContainer.GetComponent<RectTransform>().sizeDelta = new Vector3(210f, 28f);
            foodContentContainer.GetComponent<RectTransform>().sizeDelta = new Vector3(210f, 28f);

            resourcesLayout.childControlWidth = true;
            resourcesLayout.childControlHeight = true;
            resourcesLayout.childForceExpandWidth = false;
            resourcesLayout.childForceExpandHeight = false;
            resourcesLayout.childAlignment = TextAnchor.MiddleLeft;
            resourcesLayout.padding = new RectOffset(0, 2, 0, 0);
            foodLayout.childControlWidth = true;
            foodLayout.childControlHeight = true;
            foodLayout.childForceExpandWidth = false;
            foodLayout.childForceExpandHeight = false;
            foodLayout.childAlignment = TextAnchor.MiddleLeft;
            foodLayout.padding = new RectOffset(0, 2, 0, 0);

            resourcesLayoutElement.flexibleWidth = 0;
            resourcesLayoutElement.preferredWidth = 14;
            resourcesLayoutElement.preferredHeight = 14;
            foodLayoutElement.flexibleWidth = 0;
            foodLayoutElement.preferredWidth = 14;
            foodLayoutElement.preferredHeight = 14;
            resourcesGridLayoutElement.flexibleWidth = 0;
            foodGridLayoutElement.flexibleWidth = 0;
            spacerLayoutElement.flexibleWidth = 1;
            spacerLayoutElement.preferredWidth = 0;

            GameObject.Instantiate(spacerPrefab, resourcesContentContainer.transform);
            GameObject.Instantiate(spacerPrefab, foodContentContainer.transform);

            resourcesGrid.SetParent(resourcesContentContainer.transform);
            foodGrid.SetParent(foodContentContainer.transform);

            GameObject.Instantiate(spacerPrefab, resourcesContentContainer.transform);
            GameObject.Instantiate(spacerPrefab, foodContentContainer.transform);

            resourcesObject.transform.SetParent(resourcesContentContainer.transform);
            foodObject.transform.SetParent(foodContentContainer.transform);

            resourcesContentContainer.transform.SetParent(contentResourcesContainer);
            foodContentContainer.transform.SetParent(contentFoodContainer);
            GameObject.Destroy(spacerPrefab);
        }
    }
}