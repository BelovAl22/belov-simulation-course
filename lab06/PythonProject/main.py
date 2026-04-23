import tkinter as tk
from tkinter import ttk, messagebox
import random
import matplotlib.pyplot as plt
from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg
import numpy as np
from scipy import stats
from scipy.special import erf

# --- Теоретический справочник (Квантили Хи-квадрат для alpha=0.05) ---
CHI2_CRITICAL_VALUES = {
    1: 3.841, 2: 5.991, 3: 7.815, 4: 9.488, 5: 11.070,
    6: 12.592, 7: 14.067, 8: 15.507, 9: 16.919, 10: 18.307
}


class DiscreteRVSTab:
    """
    Дискретная случайная величина
    МЕТОД: Обратного преобразования (Inverse Transform Method)
    """

    def __init__(self, parent):
        self.frame = ttk.Frame(parent, padding="10")
        self.frame.pack(fill=tk.BOTH, expand=True)

        left_panel = ttk.Frame(self.frame, width=280)
        left_panel.pack(side=tk.LEFT, fill=tk.Y, padx=(0, 10))
        left_panel.pack_propagate(False)

        right_panel = ttk.Frame(self.frame)
        right_panel.pack(side=tk.RIGHT, fill=tk.BOTH, expand=True)

        # === ЛЕВАЯ ПАНЕЛЬ ===
        ttk.Label(left_panel, text="DISCRETE RANDOM VARIABLE",
                  font=('Arial', 11, 'bold')).pack(pady=(0, 10))
        ttk.Label(left_panel, text="Method: Inverse Transform",
                  font=('Arial', 9), foreground='gray').pack(pady=(0, 10))

        ttk.Label(left_panel, text="Probabilities (Σp = 1):",
                  font=('Arial', 10, 'bold')).pack(pady=(10, 5))

        self.prob_entries = []
        default_probs = [0.264, 0.128, 0.228, 0.207, 0.173]

        for i in range(5):
            frame = ttk.Frame(left_panel)
            frame.pack(fill=tk.X, pady=2)
            ttk.Label(frame, text=f"  P(X={i + 1}):", width=10).pack(side=tk.LEFT)
            entry = ttk.Entry(frame, width=10)
            entry.insert(0, str(default_probs[i]))
            entry.pack(side=tk.LEFT, padx=5)
            self.prob_entries.append(entry)

        ttk.Button(left_panel, text="Randomize Probabilities",
                   command=self.randomize_probs).pack(pady=10, fill=tk.X)

        ttk.Label(left_panel, text="\nNumber of Experiments (N):",
                  font=('Arial', 10)).pack(pady=(10, 5))
        self.entry_n = ttk.Entry(left_panel, width=12)
        self.entry_n.insert(0, "10000")
        self.entry_n.pack()

        ttk.Button(left_panel, text="▶ Start Simulation",
                   command=self.run_simulation, style='Accent.TButton').pack(pady=15, fill=tk.X)

        # === ПРАВАЯ ПАНЕЛЬ ===
        self.fig, self.ax = plt.subplots(figsize=(6.5, 4.5))
        self.canvas = FigureCanvasTkAgg(self.fig, master=right_panel)
        self.canvas.get_tk_widget().pack(fill=tk.BOTH, expand=True, pady=10)

        self.result_text = tk.Text(right_panel, height=12, font=('Consolas', 9))
        self.result_text.pack(fill=tk.X, pady=10)

    def randomize_probs(self):
        raw = [random.random() for _ in range(5)]
        total = sum(raw)
        normalized = [x / total for x in raw]
        for i, val in enumerate(normalized):
            self.prob_entries[i].delete(0, tk.END)
            self.prob_entries[i].insert(0, f"{val:.3f}")

    def generate_dsv_value(self, probs):
        """
        МЕТОД ОБРАТНОГО ПРЕОБРАЗОВАНИЯ для ДСВ
        Генерируем U ~ Uniform(0,1) и находим X по кумулятивной функции
        """
        alpha = random.random()  # U(0,1)
        cumulative_p = 0.0
        for k, p in enumerate(probs):
            cumulative_p += p
            if alpha < cumulative_p:
                return k + 1  # X = 1, 2, 3, 4, or 5
        return len(probs)

    def run_simulation(self):
        try:
            probs = [float(e.get()) for e in self.prob_entries]
            N = int(self.entry_n.get())
        except ValueError:
            messagebox.showerror("Error", "Enter valid numbers")
            return

        if abs(sum(probs) - 1.0) > 0.01:
            messagebox.showwarning("Warning", f"Probabilities sum to {sum(probs):.3f}, not 1.0")

        # Simulation
        counts = {1: 0, 2: 0, 3: 0, 4: 0, 5: 0}
        for _ in range(N):
            val = self.generate_dsv_value(probs)
            counts[val] += 1

        # Theoretical values
        exp_mean = sum((i + 1) * p for i, p in enumerate(probs))
        exp_variance = sum(((i + 1) ** 2) * p for i, p in enumerate(probs)) - (exp_mean ** 2)

        # Empirical values
        emp_mean = sum(x * (counts[x] / N) for x in counts)
        emp_variance = sum((x ** 2) * (counts[x] / N) for x in counts) - (emp_mean ** 2)

        # Errors
        err_mean = abs(emp_mean - exp_mean) / abs(exp_mean) * 100 if exp_mean != 0 else 0
        err_var = abs(emp_variance - exp_variance) / abs(exp_variance) * 100 if exp_variance != 0 else 0

        # Chi-squared test
        chi2_stat = 0
        for x in counts:
            n_obs = counts[x]
            n_exp = N * probs[x - 1]
            if n_exp > 0:
                chi2_stat += (n_obs - n_exp) ** 2 / n_exp

        dof = len(probs) - 1
        critical_val = CHI2_CRITICAL_VALUES.get(dof, 9.488)

        # Plot
        self.ax.clear()
        categories = list(counts.keys())
        frequencies = [counts[k] / N for k in categories]

        max_freq = max(frequencies)
        ylim_top = max(max_freq * 1.15 + 0.02, 0.35)
        self.ax.set_ylim(0, ylim_top)

        self.ax.bar(categories, frequencies, color='#c5d8ec', edgecolor='#3498db', width=0.8)
        self.ax.set_xticks(categories)
        self.ax.set_ylabel('Frequency', fontsize=11)
        self.ax.set_xlabel('x', fontsize=11)
        self.ax.set_title('Discrete Random Variable Distribution', fontsize=12, fontweight='bold')

        for i, v in enumerate(frequencies):
            self.ax.text(i + 1, v + (ylim_top * 0.02), f"{v:.3f}",
                         ha='center', fontweight='bold', fontsize=10)

        self.fig.tight_layout()
        self.canvas.draw()

        # Results
        self.result_text.delete(1.0, tk.END)
        self.result_text.insert(tk.END, "=" * 70 + "\n")
        self.result_text.insert(tk.END, "DISCRETE RANDOM VARIABLE - RESULTS\n")
        self.result_text.insert(tk.END, "=" * 70 + "\n\n")

        self.result_text.insert(tk.END, f"{'THEORETICAL VALUES:':<30}\n")
        self.result_text.insert(tk.END, f"  Mean (E[x]):           {exp_mean:.4f}\n")
        self.result_text.insert(tk.END, f"  Variance (D[x]):       {exp_variance:.4f}\n\n")

        self.result_text.insert(tk.END, f"{'EMPIRICAL VALUES (N=' + str(N) + '):':<30}\n")
        self.result_text.insert(tk.END, f"  Mean:                  {emp_mean:.4f} (error = {err_mean:.2f}%)\n")
        self.result_text.insert(tk.END, f"  Variance:              {emp_variance:.4f} (error = {err_var:.2f}%)\n\n")

        self.result_text.insert(tk.END, f"{'CHI-SQUARED TEST:':<30}\n")
        self.result_text.insert(tk.END, f"  χ² statistic:          {chi2_stat:.4f}\n")
        self.result_text.insert(tk.END, f"  Degrees of freedom:    {dof}\n")
        self.result_text.insert(tk.END, f"  Critical value (α=0.05): {critical_val:.4f}\n")

        if chi2_stat < critical_val:
            self.result_text.insert(tk.END, f"  ✓ H₀ ACCEPTED: Distribution fits well\n")
            self.result_text.tag_add("accept", "end-3c", "end-1c")
        else:
            self.result_text.insert(tk.END, f"  ✗ H₀ REJECTED: Distribution does not fit\n")
            self.result_text.tag_add("reject", "end-3c", "end-1c")


class NormalDistributionTab:
    """
    Нормальное распределение
    МЕТОД: Бокса-Мюллера (Box-Muller Transform)
    """

    def __init__(self, parent):
        self.frame = ttk.Frame(parent, padding="10")
        self.frame.pack(fill=tk.BOTH, expand=True)

        # Control panel
        control_frame = ttk.LabelFrame(self.frame, text="Parameters", padding="10")
        control_frame.pack(fill=tk.X, pady=(0, 10))

        ttk.Label(control_frame, text="Mean (μ):", font=('Arial', 10, 'bold')).grid(row=0, column=0, padx=10, pady=5)
        self.entry_mu = ttk.Entry(control_frame, width=10)
        self.entry_mu.insert(0, "0")
        self.entry_mu.grid(row=0, column=1, padx=5, pady=5)

        ttk.Label(control_frame, text="Std Dev (σ):", font=('Arial', 10, 'bold')).grid(row=0, column=2, padx=10, pady=5)
        self.entry_sigma = ttk.Entry(control_frame, width=10)
        self.entry_sigma.insert(0, "1")
        self.entry_sigma.grid(row=0, column=3, padx=5, pady=5)

        ttk.Label(control_frame, text="Number of bins for χ²:", font=('Arial', 10)).grid(row=0, column=4, padx=10,
                                                                                         pady=5)
        self.entry_bins = ttk.Entry(control_frame, width=8)
        self.entry_bins.insert(0, "10")
        self.entry_bins.grid(row=0, column=5, padx=5, pady=5)

        ttk.Button(control_frame, text="▶ Run Simulation", command=self.run_simulation,
                   style='Accent.TButton').grid(row=0, column=6, padx=20, pady=5)

        # Method info
        method_frame = ttk.Frame(self.frame)
        method_frame.pack(fill=tk.X, pady=(0, 10))
        ttk.Label(method_frame, text="Method: Box-Muller Transform",
                  font=('Arial', 9), foreground='gray').pack()

        # Results text
        self.result_text = tk.Text(self.frame, height=10, font=('Consolas', 9))
        self.result_text.pack(fill=tk.X, pady=(0, 10))

        # Graphs
        self.fig, self.axes = plt.subplots(2, 2, figsize=(11, 8))
        self.canvas = FigureCanvasTkAgg(self.fig, master=self.frame)
        self.canvas.get_tk_widget().pack(fill=tk.BOTH, expand=True)

    def generate_normal_box_muller(self, mu, sigma):
        """
        МЕТОД БОКСА-МЮЛЛЕРА для генерации нормальной СВ
        Z = sqrt(-2*ln(U1)) * cos(2*pi*U2)
        X = mu + sigma * Z
        """
        u1 = random.random()
        u2 = random.random()
        # Avoid log(0)
        while u1 == 0:
            u1 = random.random()

        z0 = np.sqrt(-2.0 * np.log(u1)) * np.cos(2.0 * np.pi * u2)
        return mu + sigma * z0

    def calculate_chi_squared_normal(self, samples, mu, sigma, n_bins):
        """
        Расчёт χ² для нормального распределения
        Разбиваем данные на интервалы и сравниваем с теоретическими частотами
        """
        # Create bins
        data_min = min(samples)
        data_max = max(samples)

        # Extend range slightly
        range_ext = (data_max - data_min) * 0.05
        data_min -= range_ext
        data_max += range_ext

        # Create equal-probability bins
        bin_edges = []
        for i in range(n_bins + 1):
            p = i / n_bins
            # Inverse CDF (percent point function)
            edge = stats.norm.ppf(p, mu, sigma)
            bin_edges.append(edge)

        # Ensure all data is within bins
        bin_edges[0] = data_min
        bin_edges[-1] = data_max

        # Count observed frequencies
        observed, _ = np.histogram(samples, bins=bin_edges)

        # Expected frequencies (equal for equal-probability bins)
        expected = len(samples) / n_bins

        # Calculate χ²
        chi2_stat = 0
        for obs in observed:
            if expected > 0:
                chi2_stat += (obs - expected) ** 2 / expected

        # Degrees of freedom: k - 1 - 2 (estimated mu and sigma)
        dof = n_bins - 1 - 2

        return chi2_stat, dof, observed, expected, bin_edges

    def run_simulation(self):
        try:
            mu = float(self.entry_mu.get())
            sigma = float(self.entry_sigma.get())
            n_bins = int(self.entry_bins.get())
        except ValueError:
            messagebox.showerror("Error", "Enter valid numbers")
            return

        if sigma <= 0:
            messagebox.showerror("Error", "Sigma must be positive")
        if n_bins < 5:
            messagebox.showwarning("Warning", "Number of bins should be at least 5")
            n_bins = 5

        N_values = [10, 100, 1000, 10000]
        results = []

        # Clear previous plots
        for ax in self.axes.flat:
            ax.clear()

        for idx, N in enumerate(N_values):
            # Generate samples using Box-Muller
            samples = [self.generate_normal_box_muller(mu, sigma) for _ in range(N)]
            samples_array = np.array(samples)

            # Empirical statistics
            emp_mean = np.mean(samples_array)
            emp_std = np.std(samples_array, ddof=1)
            emp_var = emp_std ** 2

            # Theoretical values
            theo_mean = mu
            theo_var = sigma ** 2

            # Errors
            err_mean = abs(emp_mean - theo_mean) / abs(theo_mean) * 100 if theo_mean != 0 else abs(
                emp_mean - theo_mean) * 100
            err_var = abs(emp_var - theo_var) / theo_var * 100

            # Chi-squared test
            if N >= n_bins * 5:  # Need enough data for chi-squared
                chi2_stat, dof, observed, expected, bin_edges = self.calculate_chi_squared_normal(
                    samples, mu, sigma, n_bins)
                critical_val = CHI2_CRITICAL_VALUES.get(max(1, dof), 9.488)
                chi2_accept = chi2_stat < critical_val
            else:
                chi2_stat = None
                dof = None
                critical_val = None
                chi2_accept = None

            results.append({
                'N': N,
                'emp_mean': emp_mean,
                'emp_var': emp_var,
                'emp_std': emp_std,
                'err_mean': err_mean,
                'err_var': err_var,
                'chi2_stat': chi2_stat,
                'dof': dof,
                'critical_val': critical_val,
                'chi2_accept': chi2_accept
            })

            # Plot histogram
            row = idx // 2
            col = idx % 2
            ax = self.axes[row, col]

            ax.hist(samples, bins='auto', density=True, alpha=0.7, color='skyblue',
                    edgecolor='black', label='Histogram')

            # Theoretical normal curve
            x = np.linspace(min(samples) - 0.5, max(samples) + 0.5, 100)
            y = stats.norm.pdf(x, mu, sigma)
            ax.plot(x, y, 'r-', linewidth=2.5, label='Theoretical PDF')

            ax.axvline(emp_mean, color='green', linestyle='--', linewidth=1.5,
                       label=f'Emp. mean={emp_mean:.2f}')
            ax.axvline(mu, color='red', linestyle=':', linewidth=1.5,
                       label=f'Theo. mean={mu}')

            title_chi2 = ""
            if chi2_stat is not None:
                status = "✓" if chi2_accept else "✗"
                title_chi2 = f"\nχ²={chi2_stat:.2f} {status}"

            ax.set_title(f'N = {N}{title_chi2}', fontsize=11, fontweight='bold')
            ax.set_xlabel('x')
            ax.set_ylabel('Density')
            ax.legend(fontsize=8)
            ax.grid(True, alpha=0.3)

        self.fig.suptitle(f'Normal Distribution N({mu}, {sigma}²) - Box-Muller Method',
                          fontsize=13, fontweight='bold', y=1.02)
        self.fig.tight_layout()
        self.canvas.draw()

        # Display results
        self.result_text.delete(1.0, tk.END)
        self.result_text.insert(tk.END, "=" * 90 + "\n")
        self.result_text.insert(tk.END, "NORMAL DISTRIBUTION SIMULATION RESULTS - BOX-MULLER METHOD\n")
        self.result_text.insert(tk.END, "=" * 90 + "\n")
        self.result_text.insert(tk.END, f"Theoretical: μ = {mu}, σ = {sigma}, σ² = {sigma ** 2:.4f}\n")
        self.result_text.insert(tk.END, "=" * 90 + "\n\n")

        header = f"{'N':<7} {'Emp Mean':<10} {'Err%':<7} {'Emp Var':<10} {'Err%':<7} {'χ²':<10} {'Result':<10}\n"
        self.result_text.insert(tk.END, header)
        self.result_text.insert(tk.END, "-" * 90 + "\n")

        for r in results:
            chi2_str = f"{r['chi2_stat']:.2f}" if r['chi2_stat'] is not None else "N/A"
            if r['chi2_accept'] is True:
                result_str = "✓ Accept"
            elif r['chi2_accept'] is False:
                result_str = "✗ Reject"
            else:
                result_str = "N/A"

            line = f"{r['N']:<7} {r['emp_mean']:<10.4f} {r['err_mean']:<7.2f} {r['emp_var']:<10.4f} {r['err_var']:<7.2f} {chi2_str:<10} {result_str:<10}\n"
            self.result_text.insert(tk.END, line)

        self.result_text.insert(tk.END, "\n" + "=" * 90 + "\n")
        self.result_text.insert(tk.END, "CONCLUSION:\n")
        self.result_text.insert(tk.END, "As N increases:\n")
        self.result_text.insert(tk.END, "  • Empirical mean and variance converge to theoretical values\n")
        self.result_text.insert(tk.END, "  • χ² test confirms goodness of fit for large N\n")
        self.result_text.insert(tk.END, "  • Histogram approaches theoretical normal curve\n")
        self.result_text.insert(tk.END, "  • This demonstrates the Law of Large Numbers\n")
        self.result_text.insert(tk.END, "=" * 90 + "\n")


class Application:
    def __init__(self, root):
        self.root = root
        self.root.title("Laboratory 6.1: Simulation of Random Variables")
        self.root.geometry("1200x800")

        # Style
        style = ttk.Style()
        style.theme_use('clam')
        style.configure('Accent.TButton', background='#3498db', foreground='white',
                        font=('Arial', 11, 'bold'), padding=5)
        style.map('Accent.TButton', background=[('active', '#2980b9')])

        # Notebook (tabs)
        self.notebook = ttk.Notebook(root)
        self.notebook.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)

        # Tab 1: Discrete Random Variable
        self.tab_dsv = DiscreteRVSTab(self.notebook)
        self.notebook.add(self.tab_dsv.frame, text="  Discrete RV (Inverse Transform)  ")

        # Tab 2: Normal Distribution
        self.tab_normal = NormalDistributionTab(self.notebook)
        self.notebook.add(self.tab_normal.frame, text="  Normal RV (Box-Muller)  ")

        # Info label
        info_frame = ttk.Frame(root)
        info_frame.pack(fill=tk.X, padx=10, pady=(0, 10))
        ttk.Label(info_frame,
                  text="Methods: Inverse Transform (Discrete) | Box-Muller (Normal) | Chi-squared test for goodness of fit",
                  font=('Arial', 9), foreground='gray').pack()


if __name__ == "__main__":
    root = tk.Tk()
    app = Application(root)
    root.mainloop()