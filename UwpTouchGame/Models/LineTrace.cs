﻿using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Boredbone.XamlTools;
using Reactive.Bindings.Extensions;

namespace UwpTouchGame.Models
{

    public class LineTrace : DisposableBase
    {
        private Subject<LineNode> LineCompletedSubject { get; }
        public IObservable<LineNode> LineCompleted => this.LineCompletedSubject.AsObservable();


        public LineTrace(IObservable<MarkerContainer> hitObservable)
        {

            var hitMarker = hitObservable.Select(x => x.Marker);

            this.LineCompletedSubject = new Subject<LineNode>().AddTo(this.Disposables);

            // Line Hit
            var lineObservable = hitMarker
                .Where(x => x.Type == MarkerType.Node || x.Type == MarkerType.Line)
                .Select(x => (ILine)x)
                .Publish()
                .RefCount();

            // Node Hit
            hitMarker
                .Where(x => x.Type == MarkerType.Node)
                .Select(x => (LineNode)x)
                .Do(x => x.Capture(lineObservable))
                .Where(x => x.Parent?.IsCaptured ?? false)
                .Subscribe(this.LineCompletedSubject)
                .AddTo(this.Disposables);
        }

    }
}
