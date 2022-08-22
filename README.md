# csharp_soem
Adventures in trying to code soem from csharp. (Not going well)

Servos are nice little things. The sound they produce when running in a well tuned machine are nothing short of poetic, as the name litteraly means "servant", they are reminescent of magical elfs carrying it's masters instructions dutifully with power and precission.
However, achieving this magic is easier said than done, there is the nasty little problem of HardRealTime measured and adjusted in nanoseconds. For us flying in the rarified atmosphere of managed code, those concepts are so close to the silicon that they look distant and abstract. Trying to grasp those concepts are not so disimilar as a sorocer trying to cast a spell from an old magic only a few can correctly cast. This is the world of mutexes, mapped memory, synchronization, registers, bit shifters and the ever-eluding preemption.

This project strives to reach the following:<br>
--Provide a high-level Csharp environment to control ethercat slaves with ability to control servos.<br>
--The library should be able to configure and compile a real-time task, written in C and be able to communicate with the running process.<br>
--The package will include a code generator to produce csharp code based on the actual network and be able to produce classes described in exi-files.<br>
--The library should serve and consume web-interfaces in JSON in order to be able to code front-end control in React, Electron, or some other web-technology.<br>
--The library should be able to standarized how data is to be collected by the real-time process in order to be able to be plotted by libraries such a Grafana.<br>

I am looking for companions on this quest, I hope if you gain something, we could go along.
Thank you.
