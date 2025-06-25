using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityTimer;
using Random = UnityEngine.Random;

public struct SoundData{
    public Vector3 pos;
    public bool loop;
    public float loopTime;
}

public class AudioManager : MonoBehaviour
{
     static Dictionary<string, List<GameObject>> isPlayingSounds;
     static Dictionary<string, Queue<SoundData>> enablePlayingSounds;
     static Dictionary<string, Queue<SoundData>> copyPlayingSounds;
     public static AudioManager _instance;
     private AudioSource bgmAudioSource;
     public int maxNum = 10;
     public int maxSamePlayNum = 1;
     public static AudioManager GetInstance()
     {
        if (_instance == null)
        {
            var obj = new GameObject("AudioManager");
            obj.AddComponent<AudioSource>();
            _instance = obj.AddComponent<AudioManager>();
            obj.transform.SetParent(_instance.transform);
            _instance.bgmAudioSource = obj.GetComponent<AudioSource>();
            _instance.bgmAudioSource.loop = true;
            _instance.bgmAudioSource.playOnAwake = false;
            DontDestroyOnLoad(_instance);
            isPlayingSounds = new Dictionary<string, List<GameObject>>();
            enablePlayingSounds = new Dictionary<string, Queue<SoundData>>();
            copyPlayingSounds = new Dictionary<string, Queue<SoundData>>();
            _instance.PlaySoundUpdate();
        }

;
        return _instance;
     }

    public void Clear()
    {
        StopAllCoroutines();
        copyPlayingSounds.Clear();
        enablePlayingSounds.Clear();
        Timer.CancelAllRegisteredTimers();
        List<GameObject> soundObjs = new List<GameObject>();
        for (int i = 0; i < transform.childCount; i++)
        {
            soundObjs.Add(transform.GetChild(i).gameObject);
        }
        foreach (var item in soundObjs)
        {
            Destroy(item);
        }
        PlaySoundUpdate();
    }

    public void PlaySoundUpdate()
    {
        StartCoroutine(PlaySounds());
    }
    private IEnumerator PlaySounds()
    {
        while (true)
        {
            Copy2CopyPlayingSounds();
            SoundData obj;
            int index = 0;
            foreach (var item in copyPlayingSounds)
            {
                for (int i = 0; i < item.Value.Count; i++)
                {
                    if(item.Value.TryDequeue(out obj))
                    {
                        RealyPlaySound(item.Key, obj.pos,obj.loop,obj.loopTime);
                    }
                }
                index++;
                if (index > 2)
                {
                    yield return null;
                }
            }
            yield return null;

        }
    }
    private void Copy2CopyPlayingSounds()
    {
        copyPlayingSounds.Clear();
        foreach (var item in enablePlayingSounds)
        {
            int restCount = item.Value.Count - maxSamePlayNum;
            for (int i = 0; i < restCount; i++)
            {
                item.Value.Dequeue();
            }
            copyPlayingSounds.Add(item.Key, item.Value);
        }
        enablePlayingSounds.Clear();
    }

    public void PlayNewSound(string soundName,Vector3 pos, bool loop = false, float time = 0)
    {
        if (enablePlayingSounds.ContainsKey(soundName))
        {
            SoundData sdata = new SoundData();
            sdata.pos = pos;
            sdata.loop = loop;
            sdata.loopTime = time;
            enablePlayingSounds[soundName].Enqueue(sdata);
        }
        else
        {
            Queue<SoundData> list = new Queue<SoundData>();
            SoundData sdata = new SoundData();
            sdata.pos = pos;
            sdata.loop = loop;
            sdata.loopTime = time;
            list.Enqueue(sdata);
            enablePlayingSounds.Add(soundName, list);
        }
    }

    private void RealyPlaySound(string soundName,Vector3 pos, bool loop = false, float time = 0)
    {

        if (isPlayingSounds.ContainsKey(soundName))
        {
            if (isPlayingSounds[soundName].Count > maxNum)
            {
                return;
            }
            else
            {
                isPlayingSounds[soundName].Add(PlaySound(soundName,pos,loop,time));
            }
        }
        else
        {
            List<GameObject> list = new List<GameObject>();
            list.Add(PlaySound(soundName,pos,loop,time));
            isPlayingSounds.Add(soundName, list);
        }
    }

    private GameObject PlaySound(string soundName, Vector3 pos, bool loop = false, float time = 0)
    {
        GameObject obj = ZGameObjectPool.Pop(soundName, () => {

            var tempObj = PrefabUtils.Instance(soundName);
            tempObj.transform.SetParent(_instance.transform);
            return tempObj;
        });
        obj.SetActive(true);
        obj.transform.position = pos;
        AudioSource audioSource = obj.GetComponent<AudioSource>();
        audioSource.loop = loop;
        float totalTime = audioSource.clip.length * 0.5f;
        if (loop)
        {
            totalTime = time;
        }
        Timer.Register(totalTime, () => {
            if (isPlayingSounds.ContainsKey(soundName))
            {
                if (isPlayingSounds[soundName].Contains(obj))
                {
                    isPlayingSounds[soundName].Remove(obj);
                }
            }
            ZGameObjectPool.Push(soundName, obj);
        });
        return obj;
    }

    public void PlayBgmSound(string soundName)
    {
        _instance.bgmAudioSource.clip = AudioUtils.GetAudio(soundName);
        _instance.bgmAudioSource.Play();
    }

    public void ResumeBgmSound()
    {
        _instance.bgmAudioSource.Pause();
    }

    public void PauseBgmSound()
    {
        _instance.bgmAudioSource.UnPause();
    }



   
}

