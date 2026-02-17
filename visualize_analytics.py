import sqlite3
import pandas as pd
import matplotlib.pyplot as plt
from datetime import datetime
import os

# Configuration
DB_PATH = os.path.join("OrderAnalytics.API", "business.db")
TARGET_USER_ID = "8801b152-1c4d-4fe6-8d62-943fe4fa40b6"


def visualize_daily_revenue():
    if not os.path.exists(DB_PATH):
        print(f"Error: Database not found at {DB_PATH}")
        return

    # 1. Connect to the database
    conn = sqlite3.connect(DB_PATH)

    # 2. Query data
    query = """
    SELECT date(OrderDate) as Date, SUM(TotalAmount) as Revenue
    FROM Orders
    WHERE UserId = ?
    GROUP BY date(OrderDate)
    ORDER BY Date
    """

    try:
        df = pd.read_sql_query(query, conn, params=(TARGET_USER_ID,))
        conn.close()

        if df.empty:
            print(f"No data found for user {TARGET_USER_ID}")
            return

        # 3. Process dates
        df["Date"] = pd.to_datetime(df["Date"])

        # 4. Plotting
        plt.figure(figsize=(12, 6))
        plt.plot(df["Date"], df["Revenue"], color="#2ecc71", linewidth=2, label="Daily Revenue")

        # Add a rolling average for smoother trend visualization
        df["RollingAvg"] = df["Revenue"].rolling(window=7).mean()
        plt.plot(df["Date"], df["RollingAvg"], color="#e74c3c", linestyle="--", label="7-Day Moving Avg")

        plt.title(f"Daily Revenue Trend (User: {TARGET_USER_ID})", fontsize=14)
        plt.xlabel("Date", fontsize=12)
        plt.ylabel("Revenue ($)", fontsize=12)
        plt.grid(True, alpha=0.3)
        plt.legend()
        plt.xticks(rotation=45)
        plt.tight_layout()

        # 5. Show or Save
        output_file = "revenue_chart.png"
        plt.savefig(output_file)
        print(f"Success! Chart saved as {output_file}")
        plt.show()

    except Exception as e:
        print(f"An error occurred: {e}")


if __name__ == "__main__":
    visualize_daily_revenue()
