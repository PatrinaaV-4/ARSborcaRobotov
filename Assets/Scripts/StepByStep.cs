using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EnhancedStepByStep : MonoBehaviour
{
    [Header("Шаги инструкции")]
    [Tooltip("Перетащите сюда все объекты в правильном порядке")]
    public GameObject[] stepObjects;

    [Header("Кнопки управления")]
    public Button nextButton;
    public Button prevButton;
    public Button resetButton; // Дополнительная кнопка сброса

    [Header("Настройки отображения")]
    [Tooltip("Если включено - показывается только текущий шаг")]
    public bool showOnlyCurrentStep = false;

    [Header("Настройки автоматизации")]
    [Tooltip("Автоматически собирать дочерние объекты при старте")]
    public bool autoCollectChildren = true;

    [Header("UI элементы")]
    [Tooltip("Текст для отображения номера шага (опционально)")]
    public Text stepCounterText;
    [Tooltip("Панель с кнопками управления")]
    public GameObject controlPanel;

    [Header("Сообщения")]
    public string firstStepMessage = "Это первый шаг";
    public string lastStepMessage = "Это последний шаг";
    public string allStepsShownMessage = "Все шаги показаны!";

    // Текущее состояние
    private int currentIndex = 0;
    private List<GameObject> allChildren = new List<GameObject>();
    private bool isInitialized = false;

    void Start()
    {
        InitializeSystem();
    }

    void InitializeSystem()
    {
        Debug.Log("=== ИНИЦИАЛИЗАЦИЯ ПОШАГОВОЙ ИНСТРУКЦИИ ===");

        // Автоматически собираем дочерние объекты если нужно
        if (autoCollectChildren && (stepObjects == null || stepObjects.Length == 0))
        {
            AutoCollectChildren();
        }

        // Проверяем наличие шагов
        if (stepObjects == null || stepObjects.Length == 0)
        {
            Debug.LogError("Нет объектов для показа! Добавьте объекты в stepObjects или включите autoCollectChildren");
            return;
        }

        Debug.Log($"Найдено шагов: {stepObjects.Length}");

        // Проверяем каждый шаг
        for (int i = 0; i < stepObjects.Length; i++)
        {
            if (stepObjects[i] == null)
            {
                Debug.LogError($"Шаг {i} равен NULL!");
                return;
            }
            Debug.Log($"Шаг {i + 1}: {stepObjects[i].name}");
        }

        // Инициализируем кнопки
        InitializeButtons();

        // Скрываем все шаги в начале
        ResetToStart();

        isInitialized = true;
        Debug.Log("=== СИСТЕМА ГОТОВА ===");
    }

    void InitializeButtons()
    {
        // Настраиваем кнопку "Вперед"
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(NextStep);
            Debug.Log("Кнопка 'Вперед' настроена");
        }
        else
        {
            Debug.LogWarning("Кнопка 'Вперед' не назначена!");
        }

        // Настраиваем кнопку "Назад"
        if (prevButton != null)
        {
            prevButton.onClick.RemoveAllListeners();
            prevButton.onClick.AddListener(PrevStep);
            Debug.Log("Кнопка 'Назад' настроена");
        }
        else
        {
            Debug.LogWarning("Кнопка 'Назад' не назначена!");
        }

        // Настраиваем кнопку "Сброс"
        if (resetButton != null)
        {
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(ResetToStart);
            Debug.Log("Кнопка 'Сброс' настроена");
        }
    }

    public void NextStep()
    {
        if (!isInitialized || stepObjects.Length == 0) return;

        Debug.Log($"--- СЛЕДУЮЩИЙ ШАГ (текущий: {currentIndex + 1}, всего: {stepObjects.Length}) ---");
        // Если мы в самом начале (ничего не показано)
        if (currentIndex == -1)
        {
            currentIndex = 0;
            ShowCurrentStep();
        }
        // Если есть следующий шаг
        else if (currentIndex < stepObjects.Length - 1)
        {
            // Показываем текущий шаг (если нужно)
            if (!showOnlyCurrentStep && currentIndex >= 0)
            {
                ShowStep(currentIndex);
            }

            currentIndex++;
            ShowCurrentStep();

            // Проверяем, не последний ли это шаг
            if (currentIndex >= stepObjects.Length - 1)
            {
                Debug.Log(lastStepMessage);
                if (stepCounterText != null)
                    stepCounterText.text = lastStepMessage;
            }
        }
        // Если достигли конца
        else
        {
            Debug.Log(allStepsShownMessage);
            if (stepCounterText != null)
                stepCounterText.text = allStepsShownMessage;
        }

        UpdateUI();
    }

    public void PrevStep()
    {
        if (!isInitialized || stepObjects.Length == 0) return;

        Debug.Log($"--- ПРЕДЫДУЩИЙ ШАГ (текущий: {currentIndex + 1}) ---");

        // Если мы не в начале
        if (currentIndex > 0)
        {
            // Скрываем текущий шаг если показываем только один
            if (showOnlyCurrentStep)
            {
                HideStep(currentIndex);
            }

            currentIndex--;
            ShowCurrentStep();

            // Проверяем, не первый ли это шаг
            if (currentIndex == 0)
            {
                Debug.Log(firstStepMessage);
                if (stepCounterText != null)
                    stepCounterText.text = firstStepMessage;
            }
        }
        // Если уже в начале
        else if (currentIndex == 0)
        {
            // Можно скрыть первый шаг или показать сообщение
            if (showOnlyCurrentStep)
            {
                HideStep(currentIndex);
                currentIndex = -1;
                Debug.Log("Все шаги скрыты");
            }
            else
            {
                Debug.Log(firstStepMessage);
                if (stepCounterText != null)
                    stepCounterText.text = firstStepMessage;
            }
        }

        UpdateUI();
    }

    void ShowCurrentStep()
    {
        if (currentIndex < 0 || currentIndex >= stepObjects.Length) return;

        Debug.Log($"Показываем шаг {currentIndex + 1}: {stepObjects[currentIndex].name}");

        if (showOnlyCurrentStep)
        {
            // Скрываем все шаги
            HideAllSteps();
            // Показываем только текущий
            ShowStep(currentIndex);
        }
        else
        {
            // Показываем все шаги до текущего включительно
            for (int i = 0; i <= currentIndex; i++)
            {
                ShowStep(i);
            }
            // Скрываем все последующие
            for (int i = currentIndex + 1; i < stepObjects.Length; i++)
            {
                HideStep(i);
            }
        }
    }

    void ShowStep(int index)
    {
        if (index < 0 || index >= stepObjects.Length) return;

        if (stepObjects[index] != null)
        {
            stepObjects[index].SetActive(true);
            Debug.Log($"Активирован шаг {index + 1}: {stepObjects[index].name}");
        }
    }

    void HideStep(int index)
    {
        if (index < 0 || index >= stepObjects.Length) return;

        if (stepObjects[index] != null)
        {
            stepObjects[index].SetActive(false);
            Debug.Log($"Скрыт шаг {index + 1}: {stepObjects[index].name}");
        }
    }

    void HideAllSteps()
    {
        if (stepObjects == null) return;

        for (int i = 0; i < stepObjects.Length; i++)
        {
            HideStep(i);
        }
    }
    void UpdateUI()
    {
        // Обновляем кнопки
        if (prevButton != null)
        {
            prevButton.interactable = currentIndex > 0;
        }

        if (nextButton != null)
        {
            nextButton.interactable = currentIndex < stepObjects.Length - 1;
        }

        // Обновляем счетчик шагов
        if (stepCounterText != null)
        {
            if (currentIndex >= 0)
            {
                stepCounterText.text = $"Шаг {currentIndex + 1} из {stepObjects.Length}";
            }
            else
            {
                stepCounterText.text = "Готов к началу";
            }
        }

        // Показываем/скрываем панель управления если нужно
        if (controlPanel != null)
        {
            controlPanel.SetActive(true);
        }
    }

    public void ResetToStart()
    {
        Debug.Log("Сброс к началу");

        HideAllSteps();
        currentIndex = -1;

        if (stepCounterText != null)
            stepCounterText.text = "Готов к началу";

        UpdateUI();
    }

    public void GoToStep(int stepNumber)
    {
        int targetIndex = stepNumber - 1;

        if (targetIndex >= 0 && targetIndex < stepObjects.Length)
        {
            currentIndex = targetIndex;
            ShowCurrentStep();
            UpdateUI();
            Debug.Log($"Перешли к шагу {stepNumber}");
        }
        else
        {
            Debug.LogError($"Неверный номер шага: {stepNumber}. Допустимо: 1-{stepObjects.Length}");
        }
    }

    [ContextMenu("Автоматически собрать дочерние объекты")]
    public void AutoCollectChildren()
    {
        allChildren.Clear();

        Debug.Log($"Поиск дочерних объектов у {gameObject.name}...");

        // Получаем все дочерние объекты
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            // Пропускаем UI элементы
            if (child.GetComponent<Canvas>() != null ||
                child.GetComponent<Button>() != null ||
                child.GetComponent<Image>() != null ||
                child.GetComponent<Text>() != null)
            {
                Debug.Log($"Пропущен UI элемент: {child.name}");
                continue;
            }

            // Пропускаем пустые объекты
            if (string.IsNullOrEmpty(child.name) || child.name == "GameObject")
            {
                Debug.Log($"Пропущен пустой объект: {child.name}");
                continue;
            }

            allChildren.Add(child);
            Debug.Log($"Добавлен объект: {child.name}");
        }

        // Сортируем по имени (можно изменить на другую логику сортировки)
        allChildren.Sort((a, b) => a.name.CompareTo(b.name));

        stepObjects = allChildren.ToArray();
        Debug.Log($"Автоматически собрано {stepObjects.Length} объектов");

        // Сбрасываем систему
        isInitialized = false;
        InitializeSystem();
    }

    [ContextMenu("Показать все шаги")]
    public void ShowAllSteps()
    {
        Debug.Log("Показываем ВСЕ шаги:");

        for (int i = 0; i < stepObjects.Length; i++)
        {
            ShowStep(i);
        }

        currentIndex = stepObjects.Length - 1;
        UpdateUI();
        Debug.Log("Все шаги показаны!");
    }

    [ContextMenu("Скрыть все шаги")]
    public void HideAll()
    {
        HideAllSteps();
        currentIndex = -1;
        UpdateUI();
        Debug.Log("Все шаги скрыты");
    }

    // Методы для внешнего доступа
    public int GetCurrentStep() { return currentIndex + 1; }
    public int GetTotalSteps() { return stepObjects != null ? stepObjects.Length : 0; }
    public bool IsLastStep() { return currentIndex >= stepObjects.Length - 1; }
    public bool IsFirstStep() { return currentIndex <= 0; }

    [ContextMenu("Тест: Показать следующий")]
    void TestNext() { NextStep(); }

    [ContextMenu("Тест: Показать предыдущий")]
    void TestPrev() { PrevStep(); }

    [ContextMenu("Тест: Перейти к шагу 1")]
    void TestStep1() { GoToStep(1); }
}
