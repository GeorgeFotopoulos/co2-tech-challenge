# co2-tech-challenge

A tech challenge designed for candidates, featuring a realistic simulation of Co2 emissions calculation.

## Preamble

We are welcome you with our co2 emissions tech challenge.
The solution consist of two Api projects.

Measurements Api - provides user's energy consumption in watts at specific time point

Emissions Api - provides c02 emissions factors in kg per watt during the hour

## Api Specifications

Our Api works with Unix timestamps (represented by long) number of seconds
starts at the Unix Epoch on January 1st, 1970 at UTC.

### Measurements Api

We encourage you to get api details from swagger/open api specifications of this Api.

#### Supported users' ids

* alpha
* betta
* gamma
* delta
* epsilon
* zeta
* eta
* theta

#### Returned data remark

Api returns data in requested time frame, with resolution from 1 to 10 seconds (depends on user).

Api has data up to the now.

For an example: You requested data from timestamp 10 to timestamp 30, for user which has 3s resolution data
you should get values for following timestamps, 12, 15, 18, 21, ... 30.

### Emissions Api

We encourage you to get api details from swagger/open api specifications of this Api

#### Returned data remark

Api returns data in requested time frame, with 15 minutes resolution.

Api has data slightly ahead (1 day) to the current time.

### Calculator Api

We expect you to expose an endpoint which accepts 'user id', 'from' and 'to' timestamps
and returns single double value - calculated total emission for the user during this timeframe.

#### Usages note

1. Api will **primarily** be used to calculate emission for multiple users **within the same timeframe**.
2. Timeframe can be quite long (up to 2 weeks)
3. TimeFrame is always sometime in the past

## Task

We expect you to create Api project called Calculator Api.
Which mainly will consume two other Apis and return total emission for requested user within requested timeframe.

### Important

**You can change existing projects codebase, but your solution will be tested against original APIs and configs.**

## Implementation

1. We DO NOT limit you to use 3d-party libraries, especially if they are DeFacto Standard solving some problems.
2. We DO NOT limit you with project structure, architecture design, naming convention
   (Except reasonable and well know in .NEt)
3. We DO encourage you using modern c# and fresh language features!

## Calculation

### Algorithm

TODO

### Example

TODO

## Chaos

To make this task more interesting, we added a bit of a chaos to our apis. Enjoy!

1. There is a chance that chaos prevents you te get response. (Measurements Api Request has a chance (30%) to fail)
2. There is a chance that chaos makes you wait longer. (Emissions Api Request has a chance (50%) to be delayed (15s) )

## Technical Challenges

### 1. Calculation Challenge

Although this calculations can be done different ways, we wanna you to think over more optimal algorithm implementation.

### 2. Chaos Challenge

As you may have read, we add some 'chaos' to our Apis, but it will be nice if your Implementation can deal with this '
chaos'

### 3. Docker Challenge

Our Apis have Docker files, but they are not working. It will be nice if you fix them, and add everything
(including you project Dockerfile) to docker-compose

## Results & Review

### How to send results

We expect to receive link to GitHub repository including current solution with Added Project from the task.

### Review criteria

Both code quality and calculation correctness (we will compare with our solution) will be reviewed by our team.

### NOTES.md

You can include any remarks, your assumptions, or other important matters concerning solution in the NOTES.md file
in the repository root.


