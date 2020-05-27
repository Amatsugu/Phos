using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
	public UIObjectButton playButton;
	public UIObjectButton optionsButton;
	public UIObjectButton quitButton;

	public GameObject loadingSplash;


	// Start is called before the first frame update
	void Start()
	{
		loadingSplash.SetActive(false);
		playButton.OnClick += () =>
		{
			loadingSplash.SetActive(true);
			gameObject.SetActive(false);
			SceneManager.LoadScene(1);
		};

		quitButton.OnClick += Application.Quit;
	}
}