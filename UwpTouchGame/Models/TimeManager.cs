using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Boredbone.XamlTools;
using Reactive.Bindings.Extensions;

namespace UwpTouchGame.Models
{
    public class TimeManager : DisposableBase
    {
        private Subject<Unit> FrameUpdatingSubject { get; }
        public IObservable<Unit> FrameUpdating => this.FrameUpdatingSubject.AsObservable();

        public long CurrentFrame { get; private set; }


        public TimeManager()
        {
            this.FrameUpdatingSubject = new Subject<Unit>().AddTo(this.Disposables);
        }

        public void Update()
        {
            this.CurrentFrame++;
            this.FrameUpdatingSubject.OnNext(Unit.Default);
        }
    }
}
