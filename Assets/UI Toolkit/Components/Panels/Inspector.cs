using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class Inspector : PanelBase, IResizible
    {
        public Inspector()
        {
            // Пример кнопки загрузки Volume
            Button loadDicomBtn = new Button(() =>
            {
                VolumeLoader.LoadVolume();
            });
            loadDicomBtn.text = "Загрузить DICOM";
            this.Add(loadDicomBtn);

            Button spawnSphereBtn = new Button(() =>
            {
                SpawnController.OpenSpawnManager(true);
            });
            spawnSphereBtn.text = "Добавить сферу";
            this.Add(spawnSphereBtn);

            // Дальше добавляем необходимые элементы интерфейса
            // ..
            // ..
        }
    }
}
