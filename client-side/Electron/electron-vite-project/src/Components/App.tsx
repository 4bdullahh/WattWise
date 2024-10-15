import { useState } from 'react';
import reactLogo from '../assets/react.svg';
import viteLogo from '/electron-vite.animate.svg';
import '../App.css';
import BarGraph from './BarGraph';

interface Reading {
  date: string;
  reading: number;
}

function App() {
  // Function to format date to dd/mm/yyyy
  const formatDate = (date: Date) => {
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();
    return `${day}/${month}/${year}`;
  };

  const [isHistoryVisible, setIsHistoryVisible] = useState(false);
  const [isReadingFormVisible, setIsReadingFormVisible] = useState(false);
  const [currentReading, setCurrentReading] = useState<number | string>('');
  const [currentDate, setCurrentDate] = useState<string>(formatDate(new Date())); // Default to today's date

  // Sample past readings
  const [readings, setReadings] = useState<Reading[]>([
    { date: '01/09/2024', reading: 250 },
    { date: '15/09/2024', reading: 270 },
    { date: '01/10/2024', reading: 290 },
  ]);

  // Function to compare two dates (in dd/mm/yyyy format)
  const isDateAfter = (date1: string, date2: string) => {
    const [day1, month1, year1] = date1.split('/').map(Number);
    const [day2, month2, year2] = date2.split('/').map(Number);
    const d1 = new Date(year1, month1 - 1, day1);
    const d2 = new Date(year2, month2 - 1, day2);

    return d1 > d2;
  };

  // Toggle visibility of the current reading form
  const toggleReadingFormVisibility = () => {
    setIsReadingFormVisible(!isReadingFormVisible);
  };

  // Toggle visibility of the past readings table
  const toggleHistoryVisibility = () => {
    setIsHistoryVisible(!isHistoryVisible);
  };

  // Handle changes to the current reading input
  const handleCurrentReadingChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setCurrentReading(e.target.value);
  };

  // Handle changes to the current date input
  const handleCurrentDateChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setCurrentDate(e.target.value);
  };

  // Handle the submission of a new current reading
  const handleSubmitCurrentReading = () => {
    if (!currentReading || isNaN(Number(currentReading))) {
      alert('Please enter a valid number for the reading.');
      return;
    }

    if (!currentDate) {
      alert('Please enter a valid date.');
      return;
    }

    const latestReading = readings[readings.length - 1];

    // Validate the date is not in the past compared to the latest reading's date
    if (!isDateAfter(currentDate, latestReading.date)) {
      alert(`Please enter a date after ${latestReading.date}.`);
      return;
    }

    // Validate the reading is not lower than the latest reading
    if (Number(currentReading) <= latestReading.reading) {
      alert(`Please enter a reading higher than the latest reading of ${latestReading.reading}.`);
      return;
    }

    // Add the new reading to the past readings (at the bottom of the list)
    const newReading = {
      date: currentDate,
      reading: Number(currentReading),
    };

    setReadings([...readings, newReading]);
    setCurrentReading(''); // Reset the input fields
    setCurrentDate(formatDate(new Date())); // Reset date to today's date
    setIsReadingFormVisible(false); // Hide the form after submission
  };

  return (
    <>
      <div>
        <h1>WattWise Smart-Meter</h1>

        {/* Pass the updated readings to the BarGraph component */}
        <BarGraph readings={readings} />

        <button onClick={toggleReadingFormVisibility}>
          {isReadingFormVisible ? 'Cancel' : 'Enter Current Reading'}
        </button>

        {/* Conditional rendering of the current reading form */}
        {isReadingFormVisible && (
          <div className="current-reading-form">
            <input
              type="number"
              value={currentReading}
              onChange={handleCurrentReadingChange}
              placeholder="Enter reading"
            />
            <input
              type="text"
              value={currentDate}
              onChange={handleCurrentDateChange}
              placeholder="Enter date (dd/mm/yyyy)"
            />
            <button onClick={handleSubmitCurrentReading}>Submit</button>
          </div>
        )}

        <button onClick={toggleHistoryVisibility}>
          {isHistoryVisible ? 'Hide Past Readings' : 'Show Past Readings'}
        </button>

        {/* Conditional rendering of the past readings table */}
        {isHistoryVisible && (
          <div className="past-readings-form">
            <h2>Past Readings</h2>
            <table>
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Reading</th>
                </tr>
              </thead>
              <tbody>
                {readings.map((reading) => (
                  <tr key={reading.date}>
                    <td>{reading.date}</td>
                    <td>{reading.reading}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </>
  );
}

export default App;
