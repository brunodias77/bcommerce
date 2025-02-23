/* eslint-disable prefer-const */
import React, { useContext, useEffect, useState } from "react";
import Search from "../components/Search";
import { ShopContext } from "../context/ShopContext";
import { Product } from "../types/product-type";
import { ChevronDown } from "lucide-react";

/**
 * @summary Componente que exibe a coleção de produtos, incluindo filtros, ordenação e paginação.
 * @returns {JSX.Element} Interface da coleção de produtos.
 */
const Collection: React.FC = () => {
    // Obtém o contexto da loja (produtos e termo de busca)
    const context = useContext(ShopContext) ?? { products: [], search: "" };
    const { products, search } = context;

    // Estados para controle de filtros e paginação
    const [category, setCategory] = useState<string[]>([]);
    const [sortType, setSortType] = useState<string>("relevant");
    const [dropdownOpen, setDropdownOpen] = useState<boolean>(false);
    const [filteredProducts, setFilteredProducts] = useState<Product[]>([]);
    const [currentPage, setCurrentPage] = useState<number>(1);
    const itemsPerPage: number = 10;

    // Lista de categorias disponíveis
    const productCategories: string[] = ["Headphones", "Cameras", "Mobiles", "Speakers", "Mouse", "Watches"];

    /**
     * @summary Alterna a seleção de um filtro de categoria.
     * @param {string} value - Categoria a ser adicionada/removida.
     * @param {React.Dispatch<React.SetStateAction<string[]>>} setState - Função de atualização do estado de categorias.
     */
    const toggleFilter = (value: string, setState: React.Dispatch<React.SetStateAction<string[]>>) => {
        setState((prev) =>
            prev.includes(value) ? prev.filter((item) => item !== value) : [...prev, value]
        );
    };

    /**
     * @summary Filtra os produtos com base na busca e categorias selecionadas.
     * @returns {Product[]} Lista de produtos filtrados.
     */
    const applyFilter = (): Product[] => {
        let filtered: Product[] = [...products];

        if (search) {
            filtered = filtered.filter((product) =>
                product.name.toLowerCase().includes(search.toLowerCase())
            );
        }

        if (category.length) {
            filtered = filtered.filter((product) => category.includes(product.category));
        }

        return filtered;
    };

    /**
     * @summary Aplica a ordenação à lista de produtos.
     * @param {Product[]} productList - Lista de produtos a ser ordenada.
     * @returns {Product[]} Lista de produtos ordenada.
     */
    const applySorting = (productList: Product[]): Product[] => {
        switch (sortType) {
            case "low":
                return [...productList].sort((a, b) => a.price - b.price);
            case "high":
                return [...productList].sort((a, b) => b.price - a.price);
            default:
                return productList;
        }
    };

    /**
     * @summary Atualiza a lista de produtos filtrados e ordenados sempre que os filtros mudam.
     */
    useEffect(() => {
        let filtered = applyFilter();
        let sorted = applySorting(filtered);

        setFilteredProducts(sorted);
        setCurrentPage(1); // Reset para primeira página ao mudar filtros
    }, [category, sortType, products, search]);

    /**
     * @summary Retorna os produtos paginados com base na página atual.
     * @returns {Product[]} Lista de produtos visíveis na página atual.
     */
    const getPaginatedProducts = (): Product[] => {
        const startIndex = (currentPage - 1) * itemsPerPage;
        return filteredProducts.slice(startIndex, startIndex + itemsPerPage);
    };

    // Calcula o número total de páginas com base no total de produtos filtrados
    const totalPages: number = Math.ceil(filteredProducts.length / itemsPerPage);

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
                                        onChange={(e) => toggleFilter(e.target.value, setCategory)}
                                        type="checkbox"
                                        value={cat}
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
                                <ul className="absolute w-full bg-white border border-slate-300 shadow-md mt-1 rounded-lg overflow-hidden z-10">
                                    {["relevant", "low", "high"].map((option) => (
                                        <li
                                            key={option}
                                            className={`px-3 py-2 cursor-pointer ${sortType === option ? "bg-gray-200" : "hover:bg-gray-100"}`}
                                            onClick={() => {
                                                setSortType(option);
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
                        {getPaginatedProducts().length > 0 ? (
                            getPaginatedProducts().map((product) => (
                                <div key={product._id} className="border p-4 rounded-md">
                                    <p>{product.name}</p>
                                    <p>${product.price}</p>
                                </div>
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


// /* eslint-disable prefer-const */
// import React, { useContext, useEffect, useState } from 'react'
// import Search from '../components/Search'
// import { ShopContext } from '../context/ShopContext';
// import { Product } from "../types/product-type";
// import { ChevronDown } from "lucide-react"; // Certifique-se que esse ícone está instalado




// const Collection: React.FC = () => {
//     const context = useContext(ShopContext) ?? { products: [], search: "" };
//     const { products, search } = context;

//     const [category, setCategory] = useState<string[]>([]);
//     const [sortType, setSortType] = useState<string>("relevant");
//     const [dropdownOpen, setDropdownOpen] = useState<boolean>(false);
//     const [filteredProducts, setFilteredProducts] = useState<Product[]>([]);
//     const [currentPage, setCurrentPage] = useState<number>(1);
//     const itemsPerPage: number = 10;

//     const productCategories: string[] = ["Headphones", "Cameras", "Mobiles", "Speakers", "Mouse", "Watches"];

//     const toggleFilter = (value: string, setState: React.Dispatch<React.SetStateAction<string[]>>) => {
//         setState((prev) =>
//             prev.includes(value)
//                 ? prev.filter((item) => item !== value)
//                 : [...prev, value]
//         );
//     };

//     const applyFilter = (): Product[] => {
//         let filtered: Product[] = [...products];

//         if (search) {
//             filtered = filtered.filter((product) =>
//                 product.name.toLowerCase().includes(search.toLowerCase())
//             );
//         }

//         if (category.length) {
//             filtered = filtered.filter((product) => category.includes(product.category));
//         }

//         return filtered;
//     };

//     const applySorting = (productList: Product[]): Product[] => {
//         switch (sortType) {
//             case "low":
//                 return [...productList].sort((a, b) => a.price - b.price);
//             case "high":
//                 return [...productList].sort((a, b) => b.price - a.price);
//             default:
//                 return productList;
//         }
//     };

//     useEffect(() => {
//         let filtered = applyFilter();
//         let sorted = applySorting(filtered);

//         setFilteredProducts(sorted);
//         setCurrentPage(1); // Resetar para a primeira página quando houver alteração nos filtros
//     }, [category, sortType, products, search]);

//     const getPaginatedProducts = (): Product[] => {
//         const startIndex = (currentPage - 1) * itemsPerPage;
//         return filteredProducts.slice(startIndex, startIndex + itemsPerPage);
//     };

//     const totalPages: number = Math.ceil(filteredProducts.length / itemsPerPage);

//     return (
//         <div className="max-padd-container !px-0">
//             <div className="flex flex-col sm:flex-row gap-8 mb-16">
//                 {/* Filter */}
//                 <div className="min-w-72 bg-primary p-4 pt-8 pl-6 lg:pl-12">
//                     <Search />
//                     <div className="pl-5 py-3 mt-4 bg-white rounded-xl">
//                         <h5 className="h5 mb-4">Categories</h5>
//                         <div className="flex flex-col gap-2 text-sm font-light">
//                             {productCategories.map((cat) => (
//                                 <label key={cat} className="flex gap-2 medium-14 text-gray-30">
//                                     <input
//                                         onChange={(e) => toggleFilter(e.target.value, setCategory)}
//                                         type="checkbox"
//                                         value={cat}
//                                         className="w-3"
//                                     />
//                                     {cat}
//                                 </label>
//                             ))}
//                         </div>
//                     </div>
//                     {/* Sort Type */}
//                     <div className="px-4 py-3 mt-6 bg-white rounded-xl relative">
//                         <h5 className="h5 mb-4">Sort By</h5>

//                         {/* Custom Dropdown */}
//                         <div className="relative w-full">
//                             <button
//                                 className="border border-slate-300 outline-none text-gray-700 medium-14 h-10 w-full rounded-lg px-3 bg-white shadow-sm flex justify-between items-center"
//                                 onClick={() => setDropdownOpen(!dropdownOpen)}
//                             >
//                                 {sortType === "relevant" ? "Relevant" : sortType === "low" ? "Low" : "High"}
//                                 <ChevronDown className="w-5 h-5 text-gray-500" />
//                             </button>

//                             {dropdownOpen && (
//                                 <ul className="absolute w-full bg-white border border-slate-300 shadow-md mt-1 rounded-lg overflow-hidden z-10">
//                                     {["relevant", "low", "high"].map((option) => (
//                                         <li
//                                             key={option}
//                                             className={`px-3 py-2 cursor-pointer ${sortType === option ? "bg-gray-200" : "hover:bg-gray-100"
//                                                 }`}
//                                             onClick={() => {
//                                                 setSortType(option);
//                                                 setDropdownOpen(false);
//                                             }}
//                                         >
//                                             {option.charAt(0).toUpperCase() + option.slice(1)}
//                                         </li>
//                                     ))}
//                                 </ul>
//                             )}
//                         </div>
//                     </div>
//                 </div>

//                 {/* Right side */}
//                 <div className="pr-5 rounded-l-xl">
//                     <div className="grid grid-cols-1 xs:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-4 gap-y-6">
//                         {getPaginatedProducts().length > 0 ? (
//                             getPaginatedProducts().map((product) => (
//                                 <div key={product._id} className="border p-4 rounded-md">
//                                     <p>{product.name}</p>
//                                     <p>${product.price}</p>
//                                 </div>
//                             ))
//                         ) : (
//                             <p>No products found for selected filters</p>
//                         )}
//                     </div>
//                     <div className="flexCenter flex-wrap gap-4 mt-14 mb-10">
//                         <button
//                             disabled={currentPage === 1}
//                             onClick={() => setCurrentPage((prev) => prev - 1)}
//                             className={`${currentPage === 1 && "opacity-50 cursor-not-allowed"} btn-secondary !py-1 !px-3`}
//                         >
//                             Previous
//                         </button>

//                         {Array.from({ length: totalPages }, (_, index) => (
//                             <button
//                                 key={index + 1}
//                                 onClick={() => setCurrentPage(index + 1)}
//                                 className={`${currentPage === index + 1 && "!bg-tertiary text-white"} btn-light !py-1 !px-3`}
//                             >
//                                 {index + 1}
//                             </button>
//                         ))}

//                         <button
//                             disabled={currentPage === totalPages}
//                             onClick={() => setCurrentPage((prev) => prev + 1)}
//                             className={`${currentPage === totalPages && "opacity-50 cursor-not-allowed"} btn-secondary !py-1 !px-3`}
//                         >
//                             Next
//                         </button>
//                     </div>
//                 </div>
//             </div>
//         </div>
//     );
// };

// export default Collection;



