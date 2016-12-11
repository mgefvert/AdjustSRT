AdjustSRT
=========

AdjustSRT lets you adjust the timing of subtitles in SRT format.

It's hard to get SRT files that exactly match the timing of the movies that you
have in your media archive, if they've been ripped from DVDs and converted, or
from other sources *cough, cough*...

AdjustSRT lets you download an SRT file and adjust the timing with millisecond
precision.

Options
-------

`AdjustSRT -i _infile_ --add _time_ --sub _time_ --scale _factor_ -o _outfile_`

The `--add` and `--sub` option allows you to adjust the timing of the subtitles, either
by specifying seconds (`--add 6.5`) or an actual timestamp (`--add 00:01:30.495`).

The `--scale` option allows you to scale the timestamps in the SRT file by a factor,
counted from the first timestamp found. For instance, `--scale 1.1` would increase the
time between subtitles with 10%, `--scale 0.8` would decrease it by 20%.

If the outfile is not specified, the processed SRT file will be echoed to the
console output.

Comments
--------

Comments or feedback to mats.gefvert@gmail.com. Thanks.
