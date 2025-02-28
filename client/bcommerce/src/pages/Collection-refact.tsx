import React, { useContext, useEffect, useState, useMemo, useCallback } from "react";
import Search from "../components/Search";
import { ShopContext } from "../context/ShopContext";
import { Product } from "../types/product-type";
import { ChevronDown } from "lucide-react";
import Item from "../components/Item";

/**
 * @summary Componente que exibe a coleção de produtos, incluindo filtros, ordenação e paginação.
 * @returns {JSX.Element} Interface da coleção de produtos.
 */
const Collection: React.FC = () => {
    // Obtém o contexto da loja (produtos e termo de busca)
    const shopContext = useContext(ShopContext);

    if (!shopContext) {
        throw new Error("ShopContext deve ser usado dentro de um ShopContextProvider.");
    }

    const { products, search } = shopContext;

    // Estados
    const [category, setCategory] = useState<string[]>([]);
    const [sortType, setSortType] = useState<"relevant" | "low" | "high">("relevant");
    const [dropdownOpen, setDropdownOpen] = useState<boolean>(false);
    const [currentPage, setCurrentPage] = useState<number>(1);
    const itemsPerPage: number = 10;

    // Lista de categorias disponíveis
    const productCategories: string[] = ["Headphones", "Cameras", "Mobiles", "Speakers", "Mouse", "Watches"];

    /**
     * @summary Alterna a seleção de um filtro de categoria.
     */
    const toggleFilter = useCallback((value: string) => {
        setCategory((prev) =>
            prev.includes(value) ? prev.filter((item) => item !== value) : [...prev, value]
        );
    }, []);

    /**
     * @summary Filtra os produtos com base na busca e categorias selecionadas.
     */
    const filteredProducts = useMemo(() => {
        let filtered = [...products];

        if (search) {
            filtered = filtered.filter((product) =>
                product.name.toLowerCase().includes(search.toLowerCase())
            );
        }

        if (category.length > 0) {
            filtered = filtered.filter((product) => category.includes(product.category));
        }

        return filtered;
    }, [products, search, category]);

    /**
     * @summary Aplica a ordenação à lista de produtos.
     */
    const sortedProducts = useMemo(() => {
        switch (sortType) {
            case "low":
                return [...filteredProducts].sort((a, b) => a.price - b.price);
            case "high":
                return [...filteredProducts].sort((a, b) => b.price - a.price);
            default:
                return filteredProducts;
        }
    }, [sortType, filteredProducts]);

    /**
     * @summary Retorna os produtos paginados com base na página atual.
     */
    const paginatedProducts = useMemo(() => {
        const startIndex = (currentPage - 1) * itemsPerPage;
        return sortedProducts.slice(startIndex, startIndex + itemsPerPage);
    }, [currentPage, sortedProducts]);

    // Calcula o número total de páginas
    const totalPages: number = Math.ceil(sortedProducts.length / itemsPerPage);

    return (
        <div className="max-padd-container !px-0">
            <div className="flex flex-col sm:flex-row gap-8 mb-16">
                {/* Seção de Filtros */}
                <div className="min-w-72 bg-primary p-4 pt-8 pl-6 lg:pl-12">
                    <Search />
                    <div className="pl-5 py-3 mt-4 bg-white rounded-xl">
                        <h5 className="h5 mb-4">Categories</h5>
                        <div className="flex flex-col gap-2 text-sm font-light">
                            {productCategories.map((cat) => (
                                <label key={cat} className="flex gap-2 medium-14 text-gray-30">
                                    <input
                                        onChange={() => toggleFilter(cat)}
                                        type="checkbox"
                                        value={cat}
                                        checked={category.includes(cat)}
                                        className="w-3"
                                    />
                                    {cat}
                                </label>
                            ))}
                        </div>
                    </div>

                    {/* Seção de Ordenação */}
                    <div className="px-4 py-3 mt-6 bg-white rounded-xl relative">
                        <h5 className="h5 mb-4">Sort By</h5>
                        <div className="relative w-full">
                            <button
                                className="border border-slate-300 outline-none text-gray-700 medium-14 h-10 w-full rounded-lg px-3 bg-white shadow-sm flex justify-between items-center"
                                onClick={() => setDropdownOpen(!dropdownOpen)}
                            >
                                {sortType === "relevant" ? "Relevant" : sortType === "low" ? "Low" : "High"}
                                <ChevronDown className="w-5 h-5 text-gray-500" />
                            </button>

                            {dropdownOpen && (
                                <ul className="absolute w-full bg-white border border-slate-300 shadow-md mt-1 rounded-lg overflow-hidden z-50">
                                    {["relevant", "low", "high"].map((option) => (
                                        <li
                                            key={option}
                                            className={`px-3 py-2 cursor-pointer ${sortType === option ? "bg-gray-200" : "hover:bg-gray-100"}`}
                                            onClick={() => {
                                                setSortType(option as "relevant" | "low" | "high");
                                                setDropdownOpen(false);
                                            }}
                                        >
                                            {option.charAt(0).toUpperCase() + option.slice(1)}
                                        </li>
                                    ))}
                                </ul>
                            )}
                        </div>
                    </div>
                </div>

                {/* Seção de Produtos */}
                <div className="pr-5 rounded-l-xl">
                    <div className="grid grid-cols-1 xs:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-4 gap-y-6">
                        {paginatedProducts.length > 0 ? (
                            paginatedProducts.map((product) => (
                                <Item key={product._id} {...product} />
                            ))
                        ) : (
                            <p>No products found for selected filters</p>
                        )}
                    </div>

                    {/* Paginação */}
                    <div className="flexCenter flex-wrap gap-4 mt-14 mb-10">
                        <button
                            disabled={currentPage === 1}
                            onClick={() => setCurrentPage((prev) => prev - 1)}
                            className={`${currentPage === 1 && "opacity-50 cursor-not-allowed"} btn-secondary !py-1 !px-3`}
                        >
                            Previous
                        </button>

                        {Array.from({ length: totalPages }, (_, index) => (
                            <button
                                key={index + 1}
                                onClick={() => setCurrentPage(index + 1)}
                                className={`${currentPage === index + 1 && "!bg-tertiary text-white"} btn-light !py-1 !px-3`}
                            >
                                {index + 1}
                            </button>
                        ))}

                        <button
                            disabled={currentPage === totalPages}
                            onClick={() => setCurrentPage((prev) => prev + 1)}
                            className={`${currentPage === totalPages && "opacity-50 cursor-not-allowed"} btn-secondary !py-1 !px-3`}
                        >
                            Next
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Collection;
