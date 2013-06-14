using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GpsTrackerServer.Tracker
{
    /// <summary>
    /// Базовый класс для всех классов, которые описывают блоки памяти
    /// </summary>
    public class DataBlock
    {
        /// <summary>
        /// Данные
        /// </summary>
        protected byte[] Data;

        /// <summary>
        /// Максимально допустимый размер массива Data
        /// </summary>
        public readonly int MaxSize;

        /// <summary>
        /// Максимально допустимый размер массива Data
        /// </summary>
        public readonly int MinSize;

        /// <summary>
        /// Инициализация блока памяти с заданными данными
        /// </summary>
        /// <param name="data">инициализационные данные</param>
        /// <param name="minSize">минимально допустимый размер данных</param>
        /// <param name="maxSize">максимально допустимый размер данных</param>
        public DataBlock(byte[] data, int minSize, int maxSize)
        {
            MinSize = minSize;
            MaxSize = maxSize;
            Data = new byte[data.Length];
            Buffer.BlockCopy(data, 0, Data, 0, data.Length);
            if (minSize > maxSize)
                throw new Exception("Неверные лимиты размеров");
            if (data.Length > MaxSize || data.Length < MinSize)
                throw new Exception(String.Format("Размер блока должен быть от {0} до {1} байт", MinSize, MaxSize));

        }

        /// <summary>
        /// Инициализация блока памяти с заданными данными
        /// </summary>
        /// <param name="data">инициализационные данные</param>
        /// <param name="size">допустимый размер данных</param>
        public DataBlock(byte[] data, int size) : this(data, size, size) { }

        /// <summary>
        /// Возвращает блок данных
        /// </summary>
        public byte[] BlockData
        {
            get
            {
                // Возвращаем копию массива, но не сам массив(чтобы нельзя было изменить данные напрямую)
                return Data.ToArray();
            }
        }


        /// <summary>
        /// Сравнение блока данных контроллера
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(System.Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            var k = obj as DataBlock;
            if ((System.Object)k == null)
            {
                return false;
            }

            return BlockData.SequenceEqual(k.BlockData);
        }

        public override int GetHashCode()
        {
            return Helper.ComputeChecksum(BlockData) * BlockData.Length;
        }

    }
}
