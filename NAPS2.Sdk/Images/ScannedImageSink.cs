﻿namespace NAPS2.Images;

public class ScannedImageSink
{
    private static TaskCompletionSource<ProcessedImage?> CreateTcs() => new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly List<TaskCompletionSource<ProcessedImage?>> _images = new()
    {
        CreateTcs()
    };

    public bool Completed { get; private set; }

    public int ImageCount
    {
        get
        {
            lock (this)
            {
                return _images.Count - 1;
            }
        }
    }

    public ScannedImageSource AsSource() => new SinkSource(this);

    public void SetCompleted()
    {
        lock (this)
        {
            if (Completed)
            {
                return;
            }
            Completed = true;
            _images.Last().SetResult(null);
        }
    }

    public void SetError(Exception ex)
    {
        if (ex == null)
        {
            throw new ArgumentNullException(nameof(ex));
        }
        lock (this)
        {
            if (Completed)
            {
                throw new InvalidOperationException("Sink is already in the completed state");
            }
            Completed = true;
            ex.PreserveStackTrace();
            _images.Last().SetException(ex);
        }
    }

    public virtual void PutImage(ProcessedImage image)
    {
        lock (this)
        {
            _images.Last().SetResult(image);
            _images.Add(CreateTcs());
        }
    }

    private class SinkSource : ScannedImageSource
    {
        private readonly ScannedImageSink _sink;
        private int _imagesRead;

        public SinkSource(ScannedImageSink sink)
        {
            _sink = sink;
        }

        public override async Task<ProcessedImage?> Next()
        {
            TaskCompletionSource<ProcessedImage?> tcs;
            lock (_sink)
            {
                if (_imagesRead >= _sink._images.Count)
                {
                    _imagesRead--;
                }
                tcs = _sink._images[_imagesRead];
            }
            var result = await tcs.Task;
            _imagesRead++;
            return result;
        }
    }
}