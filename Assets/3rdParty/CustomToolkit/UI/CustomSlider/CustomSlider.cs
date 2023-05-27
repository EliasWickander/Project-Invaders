using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class CustomSlider : MonoBehaviour, IDragHandler, IPointerDownHandler
{
	[SerializeField]
	private RectTransform m_handleRect = null;
	public RectTransform HandleRect
	{
		get
		{
			return m_handleRect;
		}
		set
		{
			m_handleRect = value;
			UpdateVisuals();
		}
	}

	[SerializeField] 
	private RectTransform m_fillContainerRect = null;
	public RectTransform FillContainerRect
	{
		get
		{
			return m_fillContainerRect;
		}
		set
		{
			m_fillContainerRect = value;
			UpdateVisuals();
		}
	}
	
	[SerializeField] 
	private RectTransform m_fillRect = null;
	public RectTransform FillRect
	{
		get
		{
			return m_fillRect;
		}
		set
		{
			m_fillRect = value;
			UpdateVisuals();
		}
	}
	
	//Current value
	[SerializeField] 
	private float m_value = 0;
	public float Value
	{
		get
		{
			return m_value;
		}
		set
		{
			Set(value);
			UpdateVisuals();
		}
	}

	//Min value of slider
	[SerializeField] 
	private float m_minValue = 0;
	public float MinValue
	{
		get
		{
			return m_minValue;
		}
		set
		{
			m_minValue = value;
			Set(m_value);
			UpdateVisuals();
		}
	}

	//Max value of slider
	[SerializeField] 
	private float m_maxValue = 1;
	public float MaxValue
	{
		get
		{
			return m_maxValue;
		}
		set
		{
			m_maxValue = value;
			Set(m_value);
			UpdateVisuals();
		}
	}

	private float m_normalizedValue;
	public UnityEvent<float> OnValueChanged;

	protected virtual void Awake()
	{
		
	}

	protected virtual void OnValidate()
	{
		Set(m_value, false);
		UpdateVisuals();
	}

	protected virtual void OnEnable()
	{
		Set(m_value, false);
		UpdateVisuals();
	}

	public void Set(float value, bool sendCallback = true)
	{
		float newValue = Mathf.Clamp(value, m_minValue, m_maxValue);

		m_normalizedValue = Mathf.InverseLerp(m_minValue, m_maxValue, newValue);
		
		if(m_value == newValue)
			return;

		m_value = newValue;
		
		if(sendCallback)
			OnValueChanged?.Invoke(value);
		
		UpdateVisuals();
	}

	private void UpdateVisuals()
	{
		m_handleRect.anchorMin = new Vector2(m_normalizedValue, 0f);
		m_handleRect.anchorMax = new Vector2(m_normalizedValue, 1f);

		m_fillRect.anchorMin = new Vector2(0f, 0f);
		m_fillRect.anchorMax = new Vector2(m_normalizedValue, 1f);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(m_fillContainerRect, eventData.position, eventData.enterEventCamera, out var localMousePos))
			return;
		
		SetOnMousePos(localMousePos);
	}

	public void OnDrag(PointerEventData eventData)
	{
		if(m_fillContainerRect == null)
			return;
		
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(m_fillContainerRect, eventData.position, eventData.enterEventCamera, out var localMousePos))
			return;

		SetOnMousePos(localMousePos);
	}

	private void SetOnMousePos(Vector2 localMousePos)
	{
		float fillContainerWidth = m_fillContainerRect.rect.width;

		float mousePercentage = Mathf.Clamp01((localMousePos.x + fillContainerWidth * 0.5f) / fillContainerWidth);

		Set(Mathf.Lerp(m_minValue, m_maxValue, mousePercentage));
	}
}
