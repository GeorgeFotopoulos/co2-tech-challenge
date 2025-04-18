# co2-tech-challenge

A tech challenge designed for candidates, featuring a realistic simulation of Co2 emissions calculation.

## Preamble

We welcome you with our co2 emissions tech challenge.
The solution consists of two Api projects.

Measurements API - provides user's energy consumption in Watts at a specific time point.

Emissions API - provides c02 emissions factors in kg per kWh during the day.

## API Specifications

Our API works with Unix timestamps (represented by long) number of seconds
starts at the Unix Epoch on January 1st, 1970 at UTC.

### Measurements API

We encourage you to get API details from Swagger/open API specifications of this API.

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

Measurements API returns data in requested time frame, with resolution from 1 to 10 seconds (depends on user).

Measurements API has data up to the now.

For example: You requested data from timestamp 10 to timestamp 30, for user which has 3s resolution data
you should get values for following timestamps, 12, 15, 18, 21, ... 30.

### Emissions API

We encourage you to get API details from Swagger/open API specifications of this API.

#### Returned data remark

API returns data in requested time frame, with 15 minutes resolution.

API has data slightly ahead (until the end of the day) of the current time.

### Calculator API

We expect you to expose an endpoint which accepts 'user id', 'from' and 'to' timestamps
and returns single double value - calculated total emission for the user during this timeframe.

#### Usages note

1. API will **primarily** be used to calculate emission for multiple users **within the same timeframe**.
2. Timeframe can be quite long (up to 2 weeks)
3. TimeFrame is always sometime in the past

## Task

We expect you to create an API project called Calculator API.
Which mainly will consume two other APIs and return the total emission for requested user within requested timeframe.

### Important

**You can change existing projects codebase, but your solution will be tested against the original APIs and configs.**

## Implementation

1. We DO NOT limit you to use 3rd-party libraries, especially if they are De Facto Standard solutions to some problems.
2. We DO NOT limit you with project structure, architecture design, naming convention
   (Except reasonable and well-known in .NET)
3. We DO encourage you to use modern c# and fresh language features!

## Calculation

### Algorithm

First step is to split the measurements in 15-minute periods and calculate the amount of energy consumption (in kWh) for each period. <br>
Then you should multiply the kwh with the corresponding co2 factor (based on the timestamp) to get the amount of co2 for each 15-minute period (in Kg). <br>
The last step is to sum them all together to get the total amount of co2 (in Kg) and return it.

### Example

E.g. user's measurement data = { <br>
14:00-14:15 = 0.5 kWh, <br>
14:15-14:30 = 1 kWh, <br>
14:30-14:45 = 2 kWh, <br>
14:45-15:00 = 0.5 kWh} <br>

*the measurements API does not return kWh, it returns watts, you should calculate the kWh based on the watts and the timestamps.

emission factor example ={ <br>
14:00-14:15 = 0.3, <br>
14:15-14:30 = 0.1, <br>
14:30-14:45 = 0.2, <br>
14:45-15:00 = 0.2 } <br>

total emissions = 0.5 * 0.3 + 1 * 0.1 + 2 * 0.2 + 0.5 * 0.2 = 0.75 kg 

## Chaos

To make this task more interesting, we added a bit of chaos to our APIs. Enjoy!

1. There is a chance that chaos prevents you from getting a response. (Measurements API Request has a chance (30%) to fail)
2. There is a chance that chaos makes you wait longer. (Emissions API Request has a chance (50%) to be delayed (15s))

## Technical Challenges

### 1. Calculation Challenge

Although these calculations can be done in many different ways, we want you to think over a more optimal algorithm implementation.

### 2. Chaos Challenge

As you may have read, we add some 'chaos' to our APIs, but it will be nice if your Implementation can deal with this 'chaos'

### 3. Docker Challenge

Our APIs have Docker files, but they are not working. It will be nice if you fix them, and add everything
(including your project Dockerfile) to docker-compose

## Results & Review

### How to send results

We expect to receive a link to the GitHub repository including the current solution with the Added Project from the task.

### Review criteria

Both code quality and calculation correctness (we will compare with our solution) will be reviewed by our team.

### NOTES.md

You can include any remarks, your assumptions, or other important matters concerning solution in the NOTES.md file
in the repository root.


